// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class VoterListsStepManager : ISingleStepManager
{
    private readonly IDbRepository<VoterList> _voterListRepo;

    public VoterListsStepManager(IDbRepository<VoterList> voterListRepo)
    {
        _voterListRepo = voterListRepo;
    }

    public Step Step => Step.VoterLists;

    public async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var hasVoterListWithDuplicates = await _voterListRepo.Query()
            .WhereHasDomainOfInfluence(domainOfInfluenceId)
            .AnyAsync(vl => vl.HasVoterDuplicates, cancellationToken: ct);

        if (hasVoterListWithDuplicates)
        {
            throw new ValidationException("Cannot complete the step while uploaded voter duplicates exists");
        }
    }

    public Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
