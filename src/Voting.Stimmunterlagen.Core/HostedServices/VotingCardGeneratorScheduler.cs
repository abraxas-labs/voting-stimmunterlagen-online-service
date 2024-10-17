// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Store;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.HostedServices;

public class VotingCardGeneratorScheduler : IScheduledJob
{
    private readonly VotingCardGeneratorLauncher _jobLauncher;
    private readonly IDbRepository<VotingCardGeneratorJob> _jobsRepo;
    private readonly IAuth _auth;
    private readonly IAuthStore _authStore;
    private readonly AppConfig _config;

    public VotingCardGeneratorScheduler(
        VotingCardGeneratorLauncher jobLauncher,
        IDbRepository<VotingCardGeneratorJob> jobsRepo,
        IAuth auth,
        IAuthStore authStore,
        AppConfig config)
    {
        _jobLauncher = jobLauncher;
        _jobsRepo = jobsRepo;
        _auth = auth;
        _authStore = authStore;
        _config = config;
    }

    public async Task Run(CancellationToken ct = default)
    {
        if (!_auth.IsAuthenticated)
        {
            _authStore.SetValues(string.Empty, new() { Loginid = _config.SecureConnect.ServiceUserId }, new() { Id = _config.SecureConnect.AbraxasTenantId }, Enumerable.Empty<string>());
        }

        var jobIds = await _jobsRepo.Query()
            .WhereInState(VotingCardGeneratorJobState.Ready)
            .WhereContestIsInTestingPhase()
            .Select(x => x.Id)
            .ToListAsync(ct);
        await _jobLauncher.RunJobs(jobIds);
    }
}
