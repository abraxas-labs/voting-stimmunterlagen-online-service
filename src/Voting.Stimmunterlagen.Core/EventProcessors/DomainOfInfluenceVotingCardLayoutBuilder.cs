// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class DomainOfInfluenceVotingCardLayoutBuilder : VotingCardLayoutBuilder<DomainOfInfluenceVotingCardLayout>
{
    private readonly DataContext _context;

    public DomainOfInfluenceVotingCardLayoutBuilder(DataContext context)
    {
        _context = context;
    }

    internal Task SyncForBasisDomainOfInfluence(Guid basisDoiId) => Sync(x => x.BasisDomainOfInfluenceId == basisDoiId);

    internal Task SyncForContest(Guid contestId) => Sync(x => x.ContestId == contestId);

    private async Task Sync(Expression<Func<ContestDomainOfInfluence, bool>> predicate)
    {
        var dois = await _context.ContestDomainOfInfluences
            .AsTracking()
            .Where(predicate)
            .Include(x => x.VotingCardLayouts)
            .Include(x => x.PoliticalBusinessPermissionEntries)
            .Include(x => x.Contest)
            .ToListAsync();

        foreach (var doi in dois)
        {
            SyncVotingCardLayouts(doi);
        }

        await _context.SaveChangesAsync();
    }

    private void SyncVotingCardLayouts(ContestDomainOfInfluence doi)
    {
        if (!doi.UsesVotingCardsInCurrentContest())
        {
            doi.VotingCardLayouts?.Clear();
            return;
        }

        SyncVotingCardLayouts(doi.VotingCardLayouts!);
    }
}
