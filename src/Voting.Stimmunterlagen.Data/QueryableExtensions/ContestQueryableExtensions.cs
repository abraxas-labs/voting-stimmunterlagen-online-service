// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class ContestQueryableExtensions
{
    public static IQueryable<Contest> WhereHasAccessToContest(this IQueryable<Contest> queryable, string tenantId)
    {
        return queryable.Where(x => x.ContestDomainOfInfluences!.Any(y => y.SecureConnectId == tenantId && y.Role != ContestRole.None));
    }

    public static IQueryable<Contest> WhereIsContestManager(this IQueryable<Contest> queryable, string tenantId)
    {
        return queryable.Where(x => x.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<T> WhereIsContestManager<T>(this IQueryable<T> queryable, string tenantId)
        where T : IHasContest
    {
        return queryable.Where(x => x.Contest!.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<T> WhereContestApproved<T>(this IQueryable<T> queryable, bool approved = true)
        where T : IHasContest
    {
        return queryable.Where(x => x.Contest!.Approved.HasValue == approved);
    }

    public static IQueryable<Contest> WhereInTestingPhase(this IQueryable<Contest> queryable, bool inTestingPhase = true)
    {
        return inTestingPhase
            ? queryable.Where(x => x.State <= ContestState.TestingPhase)
            : queryable.Where(x => x.State > ContestState.TestingPhase);
    }

    public static IQueryable<T> WhereContestInTestingPhase<T>(this IQueryable<T> queryable)
        where T : IHasContest
    {
        return queryable.Where(x => x.Contest!.State <= ContestState.TestingPhase);
    }

    public static IQueryable<T> WhereContestNotLocked<T>(this IQueryable<T> queryable)
        where T : IHasContest
    {
        return queryable.Where(x => x.Contest!.State != ContestState.PastLocked && x.Contest.State != ContestState.Archived);
    }

    public static IQueryable<T> WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast<T>(this IQueryable<T> queryable, IClock clock)
        where T : IHasContest
    {
        return queryable.Where(x =>
            x.Contest!.PrintingCenterSignUpDeadline == null ||
            x.Contest.PrintingCenterSignUpDeadline.Value > clock.UtcNow);
    }

    public static IQueryable<T> WhereContestIsNotPastPrintingCenterSignUpDeadline<T>(this IQueryable<T> queryable, IClock clock)
        where T : IHasContest
    {
        return queryable.Where(x =>
            x.Contest!.PrintingCenterSignUpDeadline != null &&
            x.Contest.PrintingCenterSignUpDeadline.Value > clock.UtcNow);
    }

    public static IQueryable<T> WhereContestIsNotPastGenerateVotingCardsDeadline<T>(this IQueryable<T> queryable, IClock clock)
    where T : IHasContest
    {
        return queryable.Where(x =>
            x.Contest!.GenerateVotingCardsDeadline != null &&
            x.Contest.GenerateVotingCardsDeadline.Value > clock.UtcNow);
    }

    public static IQueryable<Contest> WhereNotLocked(this IQueryable<Contest> queryable)
    {
        return queryable.Where(x => x.State != ContestState.PastLocked && x.State != ContestState.Archived);
    }
}
