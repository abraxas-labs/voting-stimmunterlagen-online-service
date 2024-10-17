// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class VotingCardGeneratorJobQueryableExtensions
{
    public static IQueryable<VotingCardGeneratorJob> WhereInState(
        this IQueryable<VotingCardGeneratorJob> q,
        params VotingCardGeneratorJobState[] states)
    {
        return q.Where(x => states.Contains(x.State));
    }

    public static IQueryable<VotingCardGeneratorJob> IncludeLayoutEntities(this IQueryable<VotingCardGeneratorJob> q)
    {
        return q.Include(x => x.Layout!.DomainOfInfluence!.VotingCardConfiguration)
            .Include(x => x.Layout!.DomainOfInfluence!.Contest)
            .Include(x => x.Layout!.TemplateDataFieldValues!.OrderBy(y => y.Field!.Container!.Key).ThenBy(y => y.Field!.Key))
            .ThenInclude(x => x.Field!.Container);
    }
}
