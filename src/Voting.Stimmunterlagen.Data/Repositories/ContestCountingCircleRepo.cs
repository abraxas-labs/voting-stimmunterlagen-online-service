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

public class ContestCountingCircleRepo : DbRepository<DataContext, ContestCountingCircle>
{
    public ContestCountingCircleRepo(DataContext context)
        : base(context)
    {
    }

    public Task<List<Guid>> GetIdsOfContestsInTestingPhaseByBaseId(Guid basisCcId)
    {
        return Query()
            .WhereContestInTestingPhase()
            .Where(x => x.BasisCountingCircleId == basisCcId)
            .Select(x => x.Id)
            .ToListAsync();
    }
}
