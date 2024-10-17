// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ContestVotingCardLayoutBuilder : VotingCardLayoutBuilder<ContestVotingCardLayout>
{
    private readonly DataContext _context;

    public ContestVotingCardLayoutBuilder(DataContext context)
    {
        _context = context;
    }

    internal async Task Sync(Guid id)
    {
        var contest = await _context.Contests
            .AsTracking()
            .Include(x => x.VotingCardLayouts)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(Contest), id);

        SyncVotingCardLayouts(contest.VotingCardLayouts!);

        await _context.SaveChangesAsync();
    }
}
