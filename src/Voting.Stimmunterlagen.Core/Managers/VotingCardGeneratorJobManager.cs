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
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Models;
using Voting.Lib.DmDoc.Serialization.Json;
using Voting.Lib.DocPipe;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using VoterPageInfo = Voting.Stimmunterlagen.Core.Models.VoterPageInfo;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VotingCardGeneratorJobManager
{
    private readonly IDbRepository<VotingCardGeneratorJob> _jobsRepo;
    private readonly VotingCardGeneratorLauncher _launcher;
    private readonly IAuth _auth;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _db;
    private readonly IDocPipeService _docPipeService;
    private readonly DocPipeConfig _docPipeConfig;
    private readonly IDmDocDraftCleanupQueue _draftCleanupQueue;
    private readonly ILogger<VotingCardGeneratorJobManager> _logger;
    private readonly VoterRepo _voterRepo;

    public VotingCardGeneratorJobManager(
        IDbRepository<VotingCardGeneratorJob> jobsRepo,
        IAuth auth,
        VotingCardGeneratorLauncher launcher,
        DomainOfInfluenceManager doiManager,
        DataContext db,
        IDocPipeService docPipeService,
        DocPipeConfig docPipeConfig,
        IDmDocDraftCleanupQueue draftCleanupQueue,
        ILogger<VotingCardGeneratorJobManager> logger,
        VoterRepo voterRepo)
    {
        _jobsRepo = jobsRepo;
        _auth = auth;
        _launcher = launcher;
        _doiManager = doiManager;
        _db = db;
        _docPipeService = docPipeService;
        _docPipeConfig = docPipeConfig;
        _draftCleanupQueue = draftCleanupQueue;
        _logger = logger;
        _voterRepo = voterRepo;
    }

    public async Task<List<VotingCardGeneratorJob>> ListJobs(Guid doiId, bool forPrintJobManagement)
    {
        var tenantId = forPrintJobManagement
            ? await _doiManager.GetSecureConnectId(doiId)
            : _auth.Tenant.Id;

        return await _jobsRepo.Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .WhereHasDomainOfInfluence(doiId)
            .OrderBy(x => x.State)
            .ThenBy(x => x.FileName)
            .Include(x => x.Layout)
            .ToListAsync();
    }

    public async Task RetryJobs(Guid doiId, CancellationToken ct)
    {
        var jobs = await _jobsRepo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereInState(VotingCardGeneratorJobState.Ready, VotingCardGeneratorJobState.Failed)
            .WhereHasDomainOfInfluence(doiId)
            .WhereContestIsInTestingPhase()
            .ToListAsync(ct);

        foreach (var job in jobs)
        {
            job.State = VotingCardGeneratorJobState.Ready;
        }

        await _jobsRepo.UpdateRange(jobs);

        _ = _launcher.RunJobs(jobs.Select(x => x.Id));
    }

    public async Task HandleCallback(string serializedCallbackData, string callbackToken, CancellationToken ct)
    {
        var callbackData = DmDocJsonSerializer.Deserialize<CallbackData>(serializedCallbackData);

        var draftId = callbackData.ObjectId;
        var job = await _jobsRepo.Query()
            .Where(j => j.CallbackToken == callbackToken && j.DraftId == draftId)
            .Select(j => new { JobId = j.Id, TenantId = j.DomainOfInfluence!.SecureConnectId })
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), new { callbackToken, draftId });

        if (callbackData.Action == CallbackAction.FinishEditing && callbackData.Data?.PrintJobId != null)
        {
            await UpdateVoterPageInfos(job.JobId, draftId);
            await _launcher.CompleteJob(job.JobId, job.TenantId, callbackData.Data.PrintJobId.Value, ct);
            _draftCleanupQueue.Enqueue(callbackData.ObjectId, DraftCleanupMode.Content);
            return;
        }

        await _launcher.FailJob(job.JobId, job.TenantId, callbackData.Message);
        _draftCleanupQueue.Enqueue(callbackData.ObjectId, DraftCleanupMode.Hard);
    }

    private async Task UpdateVoterPageInfos(Guid jobId, int draftId)
    {
        var voterPageInfos = await GetVoterPageInfos(draftId);
        _logger.LogInformation("Received {VoterCount} voters for job {JobId}", voterPageInfos.Count, jobId);

        var job = await _jobsRepo.Query()
            .WhereContestIsInTestingPhase()
            .FirstOrDefaultAsync(j => j.Id == jobId)
            ?? throw new EntityNotFoundException(nameof(VotingCardGeneratorJob), jobId);

        if (job.HasEmptyVotingCards)
        {
            // Empty voting cards are not persisted as "Voter" in the database, so we ignore the page infos.
            return;
        }

        var voters = await _voterRepo.Query()
            .AsTracking()
            .Where(v => v.JobId == jobId)
            .ToListAsync();

        var voterPageInfoByVoterId = voterPageInfos.ToDictionary(x => x.Id);

        foreach (var voter in voters)
        {
            if (!voterPageInfoByVoterId.TryGetValue(voter.Id, out var voterPageInfo))
            {
                throw new ValidationException($"Request does not provide voter page info of voter {voter.Id} for job {jobId}");
            }

            voter.PageInfo ??= new Data.Models.VoterPageInfo();
            voter.PageInfo.PageFrom = voterPageInfo.PageFrom;
            voter.PageInfo.PageTo = voterPageInfo.PageTo;
        }

        await _db.SaveChangesAsync();
    }

    private async Task<List<VoterPageInfo>> GetVoterPageInfos(int draftId)
    {
        var variables = new Dictionary<string, string>
        {
            [_docPipeConfig.DraftIdJobVariable] = draftId.ToString(),
        };

        var info = await _docPipeService.ExecuteJob<VoterPagesInfo>(_docPipeConfig.VoterPagesApplication, variables);
        return info?.Pages ?? new List<VoterPageInfo>();
    }
}
