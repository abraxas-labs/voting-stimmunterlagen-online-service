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
    private readonly VoterListRepo _voterListRepo;

    public VoterListsStepManager(VoterListRepo voterListRepo)
    {
        _voterListRepo = voterListRepo;
    }

    public Step Step => Step.VoterLists;

    public async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var hasVoterLists = await _voterListRepo.Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .AnyAsync(vl => vl.DomainOfInfluenceId == domainOfInfluenceId);

        if (!hasVoterLists)
        {
            throw new ValidationException("Cannot approve voter lists step if no voter list are imported");
        }
    }

    public Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
