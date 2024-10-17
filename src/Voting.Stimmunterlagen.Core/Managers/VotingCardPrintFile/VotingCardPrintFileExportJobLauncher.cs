// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class VotingCardPrintFileExportJobLauncher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VotingCardPrintFileExportJobLauncher> _logger;

    public VotingCardPrintFileExportJobLauncher(IServiceProvider serviceProvider, ILogger<VotingCardPrintFileExportJobLauncher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    internal Task RunJobs(IEnumerable<Guid> jobIds)
        => Task.WhenAll(jobIds.Select(RunJob));

    private async Task RunJob(Guid id)
    {
        try
        {
            using var scope = _serviceProvider.CreateScopeCopyAuth();
            var generator = scope.ServiceProvider.GetRequiredService<VotingCardPrintFileExportGenerator>();
            await generator.Run(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error generating voting card print file export for job {jobId}", id);
        }
    }
}
