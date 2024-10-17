// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    private readonly ContestEVotingExportJobLauncher _launcher;
    private readonly IAuth _auth;

    public ContestEVotingExportJobManager(
        IDbRepository<ContestEVotingExportJob> jobsRepo,
        IAuth auth,
        ContestEVotingExportJobLauncher launcher)
    {
        _jobsRepo = jobsRepo;
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
        var job = await _jobsRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .WhereContestInTestingPhase()
            .WhereInState(ExportJobState.Failed, ExportJobState.Completed)
            .FirstOrDefaultAsync(x => x.ContestId == contestId)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), contestId);

        job.PrepareToRun();
        await _jobsRepo.Update(job);

        _ = _launcher.RunJob(job.Id);
    }
}
