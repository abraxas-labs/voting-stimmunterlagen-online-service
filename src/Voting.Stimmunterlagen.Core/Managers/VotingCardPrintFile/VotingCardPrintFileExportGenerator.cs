// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Extensions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class VotingCardPrintFileExportGenerator
{
    private readonly IDbRepository<VotingCardPrintFileExportJob> _jobRepo;
    private readonly ILogger<VotingCardPrintFileExportGenerator> _logger;
    private readonly IClock _clock;
    private readonly IVotingCardPrintFileStore _votingCardStore;
    private readonly ApiConfig _config;
    private readonly VotingCardPrintFileBuilder _votingCardPrintFileBuilder;
    private readonly IVotingCardPrintFileExportThrottler _throttler;
    private readonly AttachmentManager _attachmentManager;
    private readonly IDbRepository<Voter> _voterRepo;
    private readonly DataContext _dbContext;

    public VotingCardPrintFileExportGenerator(
        IDbRepository<VotingCardPrintFileExportJob> jobRepo,
        ILogger<VotingCardPrintFileExportGenerator> logger,
        IClock clock,
        IVotingCardPrintFileStore votingCardStore,
        ApiConfig config,
        VotingCardPrintFileBuilder votingCardPrintFileBuilder,
        IVotingCardPrintFileExportThrottler throttler,
        AttachmentManager attachmentManager,
        IDbRepository<Voter> voterRepo,
        DataContext dbContext)
    {
        _logger = logger;
        _clock = clock;
        _votingCardStore = votingCardStore;
        _jobRepo = jobRepo;
        _config = config;
        _votingCardPrintFileBuilder = votingCardPrintFileBuilder;
        _throttler = throttler;
        _attachmentManager = attachmentManager;
        _voterRepo = voterRepo;
        _dbContext = dbContext;
    }

    internal async Task Run(Guid jobId, CancellationToken ct = default)
    {
        using var logScope = _logger.BeginScope(new { jobId, Name = "Csv generation job" });

        await _throttler.Acquire(ct);

        try
        {
            var job = await FetchAndSetRunning(jobId, ct);
            var attachments = await _attachmentManager.ListForDomainOfInfluence(job.VotingCardGeneratorJob!.DomainOfInfluenceId, false);
            var csvContent = await _votingCardPrintFileBuilder.BuildPrintFile(job.VotingCardGeneratorJob!, attachments);

            if (string.IsNullOrWhiteSpace(_config.VotingCardGenerator.MessageId))
            {
                throw new ArgumentException($"Cannot store the print file csv (job: {job.Id}), because the eai message id is empty");
            }

            await _votingCardStore.SavePrintFile(
                job.FileName,
                csvContent,
                _config.VotingCardGenerator.MessageId,
                ct);

            await SetCompleted(jobId);
        }
        catch (Exception)
        {
            await TrySetFailed(jobId);
            throw;
        }
        finally
        {
            _throttler.Release();
        }
    }

    private async Task SetCompleted(Guid jobId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var job = await _jobRepo.GetByKey(jobId)
                  ?? throw new EntityNotFoundException(nameof(VotingCardPrintFileExportJob), jobId);

        job.State = ExportJobState.Completed;
        job.Completed = _clock.UtcNow;
        job.Failed = null;
        await _jobRepo.UpdateIgnoreRelations(job);
        await transaction.CommitAsync();
    }

    private async Task TrySetFailed(Guid jobId)
    {
        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

            var job = await _jobRepo.GetByKey(jobId)
                ?? throw new EntityNotFoundException(nameof(VotingCardPrintFileExportJob), jobId);

            job.Failed = _clock.UtcNow;
            job.Completed = null;
            job.State = ExportJobState.Failed;
            await _jobRepo.UpdateIgnoreRelations(job);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "could not set failed state");
        }
    }

    private async Task<VotingCardPrintFileExportJob> FetchAndSetRunning(Guid id, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        if (!await _jobRepo.TryLockForUpdate(id))
        {
            throw new ValidationException($"cannot process job with {id} since it is locked");
        }

        var job = await _jobRepo.Query()
                      .AsNoTrackingWithIdentityResolution()
                      .Include(x => x.VotingCardGeneratorJob!.DomainOfInfluence!.VotingCardConfiguration)
                      .Include(x => x.VotingCardGeneratorJob!.DomainOfInfluence!.Contest)
                      .Include(x => x.VotingCardGeneratorJob!.DomainOfInfluence!.VoterLists!)
                        .ThenInclude(x => x.PoliticalBusinessEntries)
                      .Include(x => x.VotingCardGeneratorJob!.Layout!.OverriddenTemplate)
                      .Include(x => x.VotingCardGeneratorJob!.Layout!.DomainOfInfluenceTemplate)
                      .Include(x => x.VotingCardGeneratorJob!.Layout!.Template)
                      .FirstOrDefaultAsync(x => x.Id == id, ct)
                  ?? throw new EntityNotFoundException(nameof(VotingCardPrintFileExportJob), id);

        var config = job.VotingCardGeneratorJob!.DomainOfInfluence!.VotingCardConfiguration
            ?? throw new InvalidOperationException($"Cannot generate voting cards without a configuration for job {id}");

        // ensures that the voters have the same order as in the generated voting cards.
        if (!job.VotingCardGeneratorJob.HasEmptyVotingCards)
        {
            job.VotingCardGeneratorJob.Voter = await _voterRepo.Query()
                .AsNoTrackingWithIdentityResolution()
                .Include(v => v.List!.PoliticalBusinessEntries)
                .Include(v => v.VoterDuplicate!.Voters)
                .Where(v => v.JobId == job.VotingCardGeneratorJobId)
                .Include(v => v.DomainOfInfluences)
                .ToAsyncEnumerable()
                .OrderBySortingCriteriaAsync(config.Sorts)
                .ToListAsync(ct);
        }
        else
        {
            var voter = await _dbContext.Voters
                .FirstOrDefaultAsync(v => v.List!.DomainOfInfluenceId == job.VotingCardGeneratorJob.DomainOfInfluenceId && v.PageInfo!.PageTo > 0);

            var pagesPerVoter = voter == null
                ? 0
                : Math.Max(0, voter.PageInfo!.PageTo - voter.PageInfo.PageFrom + 1);

            job.VotingCardGeneratorJob.Voter = EmptyVoterBuilder.BuildEmptyVoters(job.VotingCardGeneratorJob.DomainOfInfluence.Bfs, job.VotingCardGeneratorJob.CountOfVoters, pagesPerVoter);
        }

        switch (job.State)
        {
            case ExportJobState.Running:
                throw new ValidationException("job is currently running");
            case ExportJobState.Completed:
                throw new ValidationException("job is already completed");
        }

        job.State = ExportJobState.Running;
        job.Started = _clock.UtcNow;
        job.Failed = null;
        job.Completed = null;
        job.Runner = Environment.MachineName;

        await _jobRepo.UpdateIgnoreRelations(job);
        await transaction.CommitAsync();
        return job;
    }
}
