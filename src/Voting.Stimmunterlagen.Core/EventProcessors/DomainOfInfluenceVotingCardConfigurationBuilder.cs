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

public class DomainOfInfluenceVotingCardConfigurationBuilder
{
    private const int DefaultSampleCount = 10;
    private readonly DataContext _context;

    public DomainOfInfluenceVotingCardConfigurationBuilder(DataContext context)
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
            .Include(x => x.VotingCardConfiguration)
            .Include(x => x.PoliticalBusinessPermissionEntries)
            .Include(x => x.Contest)
            .ToListAsync();

        foreach (var doi in dois)
        {
            SyncVotingCardConfiguration(doi);
        }

        await _context.SaveChangesAsync();
    }

    private void SyncVotingCardConfiguration(ContestDomainOfInfluence doi)
    {
        if (!doi.UsesVotingCardsInCurrentContest())
        {
            doi.VotingCardConfiguration = null;
            return;
        }

        if (doi.VotingCardConfiguration != null)
        {
            return;
        }

        doi.VotingCardConfiguration = new DomainOfInfluenceVotingCardConfiguration
        {
            SampleCount = DefaultSampleCount,
        };
    }
}
