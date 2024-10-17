// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class ContestDomainOfInfluenceRepo : DbRepository<DataContext, ContestDomainOfInfluence>
{
    public ContestDomainOfInfluenceRepo(DataContext context)
        : base(context)
    {
    }

    public Task<List<Guid>> GetIdsOfContestsInTestingPhaseByBaseId(Guid basisDoiId)
    {
        return Query()
            .WhereContestInTestingPhase()
            .Where(x => x.BasisDomainOfInfluenceId == basisDoiId)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public Task<Guid> GetIdForContest(Guid contestId, Guid basisDoiId)
    {
        return Query()
            .Where(x => x.BasisDomainOfInfluenceId == basisDoiId && x.ContestId == contestId)
            .Select(x => x.Id)
            .FirstAsync();
    }

    public async Task<Dictionary<Guid, Guid>> GetIdsByContestId(Guid contestId)
    {
        var items = await Query()
            .Where(x => x.ContestId == contestId)
            .Select(x => new { x.Id, x.BasisDomainOfInfluenceId })
            .ToListAsync();
        return items.ToDictionary(x => x.BasisDomainOfInfluenceId, x => x.Id);
    }
}
