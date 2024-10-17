// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.HostedServices;

public class VotingCardPrintFileExportScheduler : IScheduledJob
{
    private readonly VotingCardPrintFileExportJobLauncher _jobLauncher;
    private readonly IDbRepository<VotingCardPrintFileExportJob> _jobsRepo;

    public VotingCardPrintFileExportScheduler(VotingCardPrintFileExportJobLauncher jobLauncher, IDbRepository<VotingCardPrintFileExportJob> jobsRepo)
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
