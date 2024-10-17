// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class VoterQueryableExtensions
{
    public static IQueryable<Voter> WhereBelongToDomainOfInfluence(this IQueryable<Voter> q, Guid doiId)
        => q.Where(x => x.List!.DomainOfInfluenceId == doiId || x.ManualJob!.Layout.DomainOfInfluenceId == doiId);

    public static IQueryable<Voter> WhereBelongToDomainOfInfluenceOnlyVoterList(this IQueryable<Voter> q, Guid doiId)
    => q.Where(x => x.List!.DomainOfInfluenceId == doiId);

    public static IQueryable<Voter> WhereVotingCardType(this IQueryable<Voter> q, VotingCardType vcType)
        => q.Where(x => x.VotingCardType == vcType);

    public static IOrderedQueryable<Voter> OrderBy(this IQueryable<Voter> q, VotingCardSort[] sorts)
    {
        if (sorts.Length == 0)
        {
            return q.OrderBy(x => x.SourceIndex);
        }

        var sorted = q.OrderBy(_ => 1);
        foreach (var sort in sorts)
        {
            sorted = sorted.ThenBy(sort);
        }

        return sorted.ThenBy(x => x.SourceIndex).ThenBy(x => x.PersonId);
    }

    private static IOrderedQueryable<Voter> ThenBy(this IOrderedQueryable<Voter> q, VotingCardSort sort)
    {
        return sort switch
        {
            VotingCardSort.Street => q.ThenBy(x => x.Street).ThenBy(x => x.HouseNumber),
            VotingCardSort.Name => q.ThenBy(x => x.LastName).ThenBy(x => x.FirstName),
            _ => q,
        };
    }
}
