// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class EVotingStepManager : NonRevertableStepManager
{
    private readonly ContestEVotingExportJobBuilder _jobBuilder;
    private readonly ContestEVotingExportJobLauncher _launcher;

    public EVotingStepManager(
        ContestEVotingExportJobBuilder jobBuilder,
        ContestEVotingExportJobLauncher launcher)
    {
        _jobBuilder = jobBuilder;
        _launcher = launcher;
    }

    public override Step Step => Step.EVoting;

    public override async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var jobId = await _jobBuilder.PrepareJobToRun(domainOfInfluenceId, tenantId, ct);
        _ = _launcher.RunJob(jobId);
    }
}
