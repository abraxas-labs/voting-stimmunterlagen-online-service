// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.HostedServices;

public class ContestEVotingExportScheduler : IScheduledJob
{
    private readonly ContestEVotingExportJobLauncher _jobLauncher;
    private readonly IDbRepository<ContestEVotingExportJob> _jobsRepo;

    public ContestEVotingExportScheduler(ContestEVotingExportJobLauncher jobLauncher, IDbRepository<ContestEVotingExportJob> jobsRepo)
    {
        _jobLauncher = jobLauncher;
        _jobsRepo = jobsRepo;
    }

    public async Task Run(CancellationToken ct = default)
    {
        var jobIds = await _jobsRepo.Query()
            .WhereInState(ExportJobState.ReadyToRun)
            .WhereContestInTestingPhase()
            .Select(x => x.Id)
            .ToListAsync(ct);
        await _jobLauncher.RunJobs(jobIds);
    }
}
