// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class VotingCardGenerator
{
    private readonly IDbRepository<VotingCardGeneratorJob> _jobRepo;
    private readonly TemplateManager _templateManager;
    private readonly IVotingCardGeneratorThrottler _throttler;
    private readonly ILogger<VotingCardGenerator> _logger;
    private readonly IClock _clock;
    private readonly IVotingCardStore _votingCardStore;
    private readonly ApiConfig _config;
    private readonly IDbRepository<Voter> _voterRepo;
    private readonly IDbRepository<VoterList> _voterListRepo;
    private readonly DataContext _dbContext;

    public VotingCardGenerator(
        IDbRepository<VotingCardGeneratorJob> jobRepo,
        TemplateManager templateManager,
        IVotingCardGeneratorThrottler throttler,
        ILogger<VotingCardGenerator> logger,
        IClock clock,
        IVotingCardStore votingCardStore,
        ApiConfig config,
        IDbRepository<Voter> voterRepo,
        IDbRepository<VoterList> voterListRepo,
        DataContext dbContext)
    {
        _templateManager = templateManager;
        _throttler = throttler;
        _logger = logger;
        _clock = clock;
        _votingCardStore = votingCardStore;
        _jobRepo = jobRepo;
        _config = config;
        _voterRepo = voterRepo;
        _voterListRepo = voterListRepo;
        _dbContext = dbContext;
    }

    internal async Task StartJob(Guid jobId, CancellationToken ct = default)
    {
        using var logScope = _logger.BeginScope(new { jobId, Name = "pdf generation job" });

        await _throttler.Acquire(ct);

        try
        {
            var job = await FetchAndSetRunning(jobId, ct);
            _logger.LogDebug("Start PDF generation");

            var webhookUrl = _config.DmDoc.GetVotingCardPdfCallbackUrl(job.CallbackToken);
            var draftId = await _templateManager.StartPdfGeneration(
                null,
                job.Layout!,
                job.Voter,
                webhookUrl,
                ct);
            await SetDraftId(job.Id, draftId);

            if (_config.DmDoc.MockedCallback)
            {
                await Complete(job.Id, 0, ct);
            }
        }
        catch (Exception)
        {
            await TryFail(jobId);
            throw;
        }
        finally
        {
            _throttler.Release();
        }
    }

    internal async Task Complete(Guid jobId, int printJobId, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var job = await _jobRepo.Query()
            .Include(j => j.Layout!.DomainOfInfluence)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), jobId);

        var pdfStream = await _templateManager.GetPdfForPrintJob(printJobId, ct);

        await _votingCardStore.SaveVotingCards(
            job.FileName,
            pdfStream,
            GetMessageId(job),
            ct);

        job.State = VotingCardGeneratorJobState.Completed;
        job.Completed = _clock.UtcNow;
        job.Failed = null;
        await _jobRepo.UpdateIgnoreRelations(job);
        _logger.LogInformation("Voting card generator job {JobId} completed", job.Id);
        await transaction.CommitAsync();
    }

    internal async Task TryFail(Guid jobId)
    {
        try
        {
            await Fail(jobId);
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "could not set failed state");
        }
    }

    internal async Task Fail(Guid jobId, string? errorMessage = null)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var job = await _jobRepo.GetByKey(jobId)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), jobId);

        job.Failed = _clock.UtcNow;
        job.Completed = null;
        job.State = VotingCardGeneratorJobState.Failed;
        await _jobRepo.Update(job);
        await transaction.CommitAsync();
        _logger.LogWarning("Voting card generator job {JobId} failed: {ErrorMessage}", job.Id, errorMessage);
    }

    private async Task SetDraftId(Guid jobId, int draftId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var job = await _jobRepo.GetByKey(jobId)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), jobId);

        job.DraftId = draftId;
        await _jobRepo.UpdateIgnoreRelations(job);
        _logger.LogInformation("Set draft ID of voting card generator job {JobId} to {DraftId}", jobId, draftId);
        await transaction.CommitAsync();
    }

    private async Task<VotingCardGeneratorJob> FetchAndSetRunning(Guid id, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        if (!await _jobRepo.TryLockForUpdate(id))
        {
            throw new ValidationException($"cannot process job with {id} since it is locked");
        }

        var job = await _jobRepo.Query()
            .AsNoTrackingWithIdentityResolution()
            .IncludeLayoutEntities()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), id);

        var config = job.Layout!.DomainOfInfluence!.VotingCardConfiguration ??
            throw new InvalidOperationException($"Cannot generate voting cards without a configuration for job {id}");

        switch (job.State)
        {
            case VotingCardGeneratorJobState.Running:
                throw new ValidationException("job is currently running");
            case VotingCardGeneratorJobState.Completed:
                throw new ValidationException("job is already completed");
            case VotingCardGeneratorJobState.ReadyToRunOffline:
                throw new ValidationException("cannot run job online which can only run offline");
        }

        job.Voter = await _voterRepo.Query()
            .Where(v => v.JobId == id)
            .OrderBy(config.Sorts)
            .ToListAsync();

        // load voter lists separately for memory reasons.
        // it uses significantly less memory than SplitQuery() because it does not load multiple instances of the same entity.
        // it also uses significantly less memory than AsNoTrackingWithIdentityResolution()
        var voterLists = await _voterListRepo.Query()
            .Where(vl => vl.DomainOfInfluenceId == job.DomainOfInfluenceId)
            .ToDictionaryAsync(x => x.Id);

        foreach (var voter in job.Voter)
        {
            if (voter.ListId == null)
            {
                continue;
            }

            voter.List = voterLists.GetValueOrDefault(voter.ListId.Value);
        }

        job.State = VotingCardGeneratorJobState.Running;
        job.Started = _clock.UtcNow;
        job.Runner = Environment.MachineName;
        job.CallbackToken = Guid.NewGuid().ToString();

        await _jobRepo.UpdateIgnoreRelations(job);
        await transaction.CommitAsync();
        return job;
    }

    private string GetMessageId(VotingCardGeneratorJob job)
    {
        var messageId = job.Layout!.DomainOfInfluence?.ExternalPrintingCenter == true
            ? job.Layout.DomainOfInfluence.ExternalPrintingCenterEaiMessageType
            : _config.VotingCardGenerator.MessageId;

        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException($"Cannot store the voting card PDF (job: {job.Id}) because the EAI message ID is empty");
        }

        return messageId;
    }
}
