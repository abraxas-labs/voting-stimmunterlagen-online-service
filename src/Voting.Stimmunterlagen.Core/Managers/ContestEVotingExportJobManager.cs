// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ContestEVotingExportJobManager
{
    private readonly IDbRepository<ContestEVotingExportJob> _jobsRepo;
    private readonly IDbRepository<StepState> _stepStateRepo;
    private readonly ContestEVotingExportJobLauncher _launcher;
    private readonly IAuth _auth;

    public ContestEVotingExportJobManager(
        IDbRepository<ContestEVotingExportJob> jobsRepo,
        IDbRepository<StepState> stepStateRepo,
        IAuth auth,
        ContestEVotingExportJobLauncher launcher)
    {
        _jobsRepo = jobsRepo;
        _stepStateRepo = stepStateRepo;
        _auth = auth;
        _launcher = launcher;
    }

    public async Task<ContestEVotingExportJob> GetJob(Guid contestId)
    {
        return await _jobsRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.ContestId == contestId)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), contestId);
    }

    public async Task RetryJob(Guid contestId)
    {
        var eVotingStepCompleted = await _stepStateRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .Where(s => s.Approved && s.Step == Step.EVoting && s.DomainOfInfluence!.ContestId == contestId)
            .AnyAsync();

        if (!eVotingStepCompleted)
        {
            throw new ForbiddenException("Cannot retry a job if the e-voting step is not approved yet or the user has no permissions");
        }

        var job = await _jobsRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .WhereContestInTestingPhase()
            .WhereInState(ExportJobState.Pending, ExportJobState.Failed, ExportJobState.Completed)
            .FirstOrDefaultAsync(x => x.ContestId == contestId)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), contestId);

        job.PrepareToRun();
        await _jobsRepo.Update(job);
        _ = _launcher.RunJob(job.Id);
    }

    public async Task UpdateAndResetJob(Guid contestId, Ech0045Version version)
    {
        var job = await _jobsRepo.Query()
            .Include(x => x.Contest!.DomainOfInfluence)
            .Include(x => x.Contest!.Translations)
            .WhereIsContestManager(_auth.Tenant.Id)
            .WhereContestInTestingPhase()
            .FirstOrDefaultAsync(x => x.ContestId == contestId)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), contestId);

        job.Reset();
        job.Ech0045Version = version;
        job.FileName = ContestEVotingExportJobBuilder.BuildFileName(job.Contest!, job.Ech0045Version);
        job.Contest = null;

        await _jobsRepo.Update(job);
    }
}
