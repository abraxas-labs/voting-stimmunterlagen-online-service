// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
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
        var basisDomainOfInfluenceId = await _context.Contests
            .Where(x => x.Id == id)
            .Select(x => (Guid?)x.DomainOfInfluence!.BasisDomainOfInfluenceId)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(Contest), id);

        var basisDomainOfInfluence = await _context.DomainOfInfluences
            .Include(x => x.PrintData)
            .FirstOrDefaultAsync(x => x.Id == basisDomainOfInfluenceId);

        // A basis domain of influence could already be deleted, even though the contest still exists (due to domain of influence snapshots).
        if (basisDomainOfInfluence == null)
        {
            return;
        }

        var contest = await _context.Contests
            .AsTracking()
            .Include(x => x.VotingCardLayouts)
            .Include(x => x.DomainOfInfluence)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(Contest), id);

        SyncVotingCardLayouts(contest.VotingCardLayouts!, basisDomainOfInfluence.PrintData);

        await _context.SaveChangesAsync();
    }
}
