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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContestEVotingExportJobLauncher> _logger;

    public ContestEVotingExportJobLauncher(IServiceProvider serviceProvider, ILogger<ContestEVotingExportJobLauncher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    internal Task RunJobs(IEnumerable<Guid> jobIds)
        => Task.WhenAll(jobIds.Select(RunJob));

    internal async Task RunJob(Guid id)
    {
        try
        {
            using var scope = _serviceProvider.CreateScopeCopyAuth();
            var generator = scope.ServiceProvider.GetRequiredService<ContestEVotingExportGenerator>();
            await generator.Run(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error generating e-voting export for job {jobId}", id);
        }
    }
}
