// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public abstract class NonRevertableStepManager : ISingleStepManager
{
    public abstract Step Step { get; }

    public abstract Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct);

    public Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
        => throw new ValidationException($"cannot revert {Step}");
}
