// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class LayoutQueryableExtensions
{
    public static IQueryable<T> WhereIsOwner<T>(
        this IQueryable<T> q,
        string tenantId)
        where T : IHasDomainOfInfluenceVotingCardLayout
    {
        return q.Where(x => x.Layout.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<T> WhereContestInTestingPhase<T>(
        this IQueryable<T> q)
        where T : IHasDomainOfInfluenceVotingCardLayout
    {
        return q.Where(x => x.Layout.DomainOfInfluence!.Contest!.State == ContestState.TestingPhase);
    }

    public static IQueryable<T> WhereHasDomainOfInfluence<T>(
        this IQueryable<T> q,
        Guid doiId)
        where T : IHasDomainOfInfluenceVotingCardLayout
    {
        return q.Where(x => x.Layout.DomainOfInfluenceId == doiId);
    }
}
