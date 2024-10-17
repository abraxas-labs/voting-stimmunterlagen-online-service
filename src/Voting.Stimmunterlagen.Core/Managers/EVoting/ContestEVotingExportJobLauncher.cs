// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class ContestEVotingExportJobLauncher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ContestEVotingExportJobLauncher> _logger;

    public ContestEVotingExportJobLauncher(IServiceScopeFactory scopeFactory, ILogger<ContestEVotingExportJobLauncher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    internal Task RunJobs(IEnumerable<Guid> jobIds)
        => Task.WhenAll(jobIds.Select(RunJob));

    internal async Task RunJob(Guid id)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<ContestEVotingExportGenerator>();
            await generator.Run(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error generating e-voting export for job {jobId}", id);
        }
    }
}
