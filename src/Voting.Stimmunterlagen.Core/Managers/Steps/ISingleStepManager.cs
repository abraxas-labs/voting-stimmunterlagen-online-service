// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public interface ISingleStepManager
{
    Step Step { get; }

    Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct);

    Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct);
}
