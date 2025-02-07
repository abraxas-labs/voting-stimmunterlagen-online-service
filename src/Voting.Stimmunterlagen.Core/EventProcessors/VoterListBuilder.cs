// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class VoterListBuilder
{
    private readonly DataContext _dataContext;

    public VoterListBuilder(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    internal async Task CleanUp(List<Guid> contestIds)
    {
        var voterListsToDelete = await _dataContext.VoterLists
            .Where(vl => contestIds.Contains(vl.DomainOfInfluence!.ContestId) &&
                ((!vl.DomainOfInfluence!.Contest!.IsPoliticalAssembly && vl.PoliticalBusinessEntries!.Count == 0) || vl.DomainOfInfluence!.StepStates!.Count(s => s.Step == Step.VoterLists) == 0))
            .ToListAsync();

        _dataContext.VoterLists.RemoveRange(voterListsToDelete);
        await _dataContext.SaveChangesAsync();
    }
}
