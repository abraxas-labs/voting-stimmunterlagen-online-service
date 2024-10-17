// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class VotingCardGeneratorLauncher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VotingCardGeneratorLauncher> _logger;

    public VotingCardGeneratorLauncher(IServiceProvider serviceProvider, ILogger<VotingCardGeneratorLauncher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    internal Task RunJobs(IEnumerable<Guid> jobIds)
        => Task.WhenAll(jobIds.Select(RunJob));

    internal async Task CompleteJob(Guid jobId, string tenantId, int printJobId, CancellationToken ct)
    {
        _logger.LogInformation(
            "Completing voting card generator job {JobId} for tenant {Tenant} with print job {PrintJobId}",
            jobId,
            tenantId,
            printJobId);

        using var scope = _serviceProvider.CreateScopeWithTenant(tenantId);
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();

        try
        {
            await generator.Complete(jobId, printJobId, ct);
        }
        catch
        {
            await generator.TryFail(jobId);
            throw;
        }
    }

    internal async Task FailJob(Guid jobId, string tenantId, string errorMessage)
    {
        using var scope = _serviceProvider.CreateScopeWithTenant(tenantId);
        var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();
        await generator.Fail(jobId, errorMessage);
    }

    private async Task RunJob(Guid id)
    {
        try
        {
            using var scope = _serviceProvider.CreateScopeCopyAuth();
            var generator = scope.ServiceProvider.GetRequiredService<VotingCardGenerator>();
            await generator.StartJob(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "error generating voting card for job {jobId}", id);
        }
    }
}
