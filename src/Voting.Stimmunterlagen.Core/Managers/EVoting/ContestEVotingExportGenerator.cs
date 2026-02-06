// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.FilterExpressions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Ech.Converter;
using Voting.Stimmunterlagen.EVoting;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using DomainOfInfluence = Voting.Stimmunterlagen.EVoting.Models.DomainOfInfluence;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class ContestEVotingExportGenerator
{
    private const string XmlFileExtension = ".xml";

    private readonly IClock _clock;
    private readonly EVotingContestBuilder _eVotingContestBuilder;
    private readonly IDbRepository<VoterList> _voterListRepo;
    private readonly IDbRepository<ContestEVotingExportJob> _jobRepo;
    private readonly ILogger<ContestEVotingExportGenerator> _logger;
    private readonly IContestEVotingStore _eVotingStore;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly IContestEVotingExportThrottler _throttler;
    private readonly ApiConfig _apiConfig;
    private readonly IDbRepository<Attachment> _attachmentRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly Ech0045Service _ech0045Service;
    private readonly IEVotingZipStorage _eVotingZipStorage;
    private readonly DataContext _dbContext;

    public ContestEVotingExportGenerator(
        IClock clock,
        EVotingContestBuilder eVotingContestBuilder,
        IDbRepository<VoterList> voterListRepo,
        IDbRepository<ContestEVotingExportJob> jobRepo,
        ILogger<ContestEVotingExportGenerator> logger,
        IContestEVotingStore eVotingStore,
        DomainOfInfluenceManager doiManager,
        IContestEVotingExportThrottler throttler,
        ApiConfig apiConfig,
        IDbRepository<Attachment> attachmentRepo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        Ech0045Service ech0045Service,
        IEVotingZipStorage eVotingZipStorage,
        DataContext dbContext)
    {
        _clock = clock;
        _eVotingContestBuilder = eVotingContestBuilder;
        _voterListRepo = voterListRepo;
        _jobRepo = jobRepo;
        _logger = logger;
        _eVotingStore = eVotingStore;
        _doiManager = doiManager;
        _throttler = throttler;
        _apiConfig = apiConfig;
        _attachmentRepo = attachmentRepo;
        _doiRepo = doiRepo;
        _ech0045Service = ech0045Service;
        _eVotingZipStorage = eVotingZipStorage;
        _dbContext = dbContext;
    }

    public async Task Run(Guid jobId, CancellationToken ct = default)
    {
        using var logScope = _logger.BeginScope(new { jobId, Name = "contest evoting export job" });

        await _throttler.Acquire(ct);

        try
        {
            var (job, contest, contestDomainOfInfluence) = await FetchJobAndSetRunning(jobId, ct);
            if (string.IsNullOrWhiteSpace(contestDomainOfInfluence.CantonDefaults.VotingDocumentsEVotingEaiMessageType))
            {
                throw new ArgumentException($"Canton settings {nameof(CantonSettings.VotingDocumentsEVotingEaiMessageType)} may not be empty");
            }

            var voterLists = await FetchVoterLists(contest);
            var attachments = await FetchAttachments(job.ContestId);
            var ech0045XmlBytes = await GetEch0045XmlBytes(job.Ech0045Version, contest, contestDomainOfInfluence.Canton, voterLists);

            var eVotingContest = await _eVotingContestBuilder.BuildContest(contest, attachments, voterLists);
            var testDomainOfInfluences = _apiConfig.ContestEVotingExport.TestDomainOfInfluences
                .GetValueOrDefault(contestDomainOfInfluence.Canton, new List<DomainOfInfluence>());

            var eVotingZipBytes = await _eVotingZipStorage.Fetch();

            var exportContent = EVotingExportDataBuilder.BuildEVotingExport(
                eVotingZipBytes,
                eVotingContest,
                ech0045XmlBytes,
                GetEch0045XmlFileName(job.FileName),
                testDomainOfInfluences,
                _apiConfig.ContestEVotingExport.TestDomainOfInfluenceDefaults,
                _apiConfig.ContestEVotingExport.EVotingDomainOfInfluences);

            await _eVotingStore.SaveContestEVotingExport(
                job.FileName,
                exportContent,
                contestDomainOfInfluence.CantonDefaults.VotingDocumentsEVotingEaiMessageType,
                ct);

            await SetCompleted(jobId, GetFileHash(exportContent));
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

    private async Task<(ContestEVotingExportJob Job, Contest Contest, ContestDomainOfInfluence ContestDomainOfInfluence)> FetchJobAndSetRunning(Guid id, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        if (!await _jobRepo.TryLockForUpdate(id))
        {
            throw new ValidationException($"cannot process job with {id} since it is locked");
        }

        var job = await _jobRepo.Query()
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(x => x.Contest!.ContestDomainOfInfluences!
                .AsQueryable()
                .Where(ContestDomainOfInfluenceFilterExpressions.InEVotingExportFilter) // nested queryables do not support extension methods
                .OrderBy(doi => doi.Bfs)
                .ThenBy(doi => doi.Name))
                .ThenInclude(x => x.CountingCircles!)
                .ThenInclude(x => x.CountingCircle)
            .Include(x => x.Contest!.Translations)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportGenerator), id);

        switch (job.State)
        {
            case ExportJobState.Unspecified:
                throw new ValidationException("job state is unspecified");
            case ExportJobState.Pending:
                throw new ValidationException("job is inactive");
            case ExportJobState.Running:
                throw new ValidationException("job is currently running");
            case ExportJobState.Completed:
                throw new ValidationException("job is already completed");
        }

        job.State = ExportJobState.Running;
        job.Started = _clock.UtcNow;
        job.Runner = Environment.MachineName;

        await _jobRepo.UpdateIgnoreRelations(job);

        var contest = job.Contest!;
        var doi = await _doiRepo.Query().FirstOrDefaultAsync(x => x.Id == contest.DomainOfInfluenceId, ct)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), job.Contest!.DomainOfInfluenceId ?? Guid.Empty);

        await transaction.CommitAsync();
        return (job, contest, doi);
    }

    private async Task<byte[]> GetEch0045XmlBytes(Ech0045Version version, Contest contest, DomainOfInfluenceCanton canton, IReadOnlyCollection<VoterList> voterLists)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var domainOfInfluenceIds = contest.ContestDomainOfInfluences?
            .Select(x => x.Id)
            .ToList()
            ?? new List<Guid>();

        var voters = new List<Voter>();
        var accumulatedVoterList = new VoterList
        {
            VotingCardType = VotingCardType.EVoting,
        };

        foreach (var voterList in voterLists)
        {
            // The accumulated voter list should not include duplicates.
            accumulatedVoterList.NumberOfVoters += voterList.CountOfVotingCards;
            voters.AddRange(voterList.Voters ?? Array.Empty<Voter>());
        }

        accumulatedVoterList.Voters = voters;

        if (accumulatedVoterList.NumberOfVoters != voters.Count)
        {
            throw new ArgumentException($"The accumulated voter list has a different {nameof(VoterList.NumberOfVoters)} ({accumulatedVoterList.NumberOfVoters}) then voters ({voters.Count})");
        }

        var doiHierarchyByDoiId = await _doiManager.GetParentsAndSelfPerDoi(domainOfInfluenceIds);

        await transaction.CommitAsync();
        return _ech0045Service.WriteEch0045Xml(version, contest, accumulatedVoterList, canton, doiHierarchyByDoiId);
    }

    private async Task<List<VoterList>> FetchVoterLists(Contest contest)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var doiIds = contest.ContestDomainOfInfluences?.Select(x => x.Id).ToList() ?? new List<Guid>();

        var voterLists = await _voterListRepo.Query()
            .AsSplitQuery()
            .Where(vl => doiIds.Contains(vl.DomainOfInfluenceId) && vl.VotingCardType == VotingCardType.EVoting && vl.DomainOfInfluence!.GenerateVotingCardsTriggered.HasValue)
            .Include(vl => vl.Voters!.Where(v => v.JobId.HasValue))
            .Include(vl => vl.PoliticalBusinessEntries)
            .Include(vl => vl.DomainOfInfluence!.CountingCircles!).ThenInclude(doiCc => doiCc.CountingCircle)
            .ToListAsync();
        await transaction.CommitAsync();
        return voterLists;
    }

    private async Task<List<Attachment>> FetchAttachments(Guid contestId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var attachments = await _attachmentRepo.Query()
            .Where(a => a.Station != null)
            .WhereIsInContest(contestId)
            .Include(a => a.DomainOfInfluenceAttachmentCounts)
            .Include(a => a.PoliticalBusinessEntries)
            .ToListAsync();
        await transaction.CommitAsync();
        return attachments;
    }

    private async Task SetCompleted(Guid jobId, string fileHash)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var job = await _jobRepo.GetByKey(jobId)
                  ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), jobId);

        job.State = ExportJobState.Completed;
        job.Completed = _clock.UtcNow;
        job.Failed = null;
        job.FileHash = fileHash;
        await _jobRepo.UpdateIgnoreRelations(job);
        await transaction.CommitAsync();
    }

    private async Task TrySetFailed(Guid jobId)
    {
        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
            var job = await _jobRepo.GetByKey(jobId)
                ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), jobId);

            job.Failed = _clock.UtcNow;
            job.Completed = null;
            job.State = ExportJobState.Failed;
            await _jobRepo.UpdateIgnoreRelations(job);
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "could not set job failed state");
        }
    }

    private string GetFileHash(byte[] exportContent)
    {
        using var sha512 = SHA512.Create();
        var exportHash = sha512.ComputeHash(exportContent);
        return Convert.ToBase64String(exportHash);
    }

    private string GetEch0045XmlFileName(string eVotingZipFileName) =>
        Path.GetFileNameWithoutExtension(eVotingZipFileName) + XmlFileExtension;
}
