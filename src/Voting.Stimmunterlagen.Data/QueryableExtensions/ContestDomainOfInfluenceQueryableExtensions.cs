// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data.FilterExpressions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class ContestDomainOfInfluenceQueryableExtensions
{
    public static IQueryable<ContestDomainOfInfluence> WhereCanRead(
        this IQueryable<ContestDomainOfInfluence> queryable,
        string tenantId)
    {
        return queryable.Where(x => x.SecureConnectId == tenantId
                                    || x.HierarchyEntries!.Any(y => y.ParentDomainOfInfluence!.SecureConnectId == tenantId));
    }

    public static IQueryable<ContestDomainOfInfluence> WhereIsManager(
        this IQueryable<ContestDomainOfInfluence> queryable,
        string tenantId)
    {
        return queryable.Where(x => x.SecureConnectId == tenantId);
    }

    public static IQueryable<T> WhereIsDomainOfInfluenceManager<T>(
        this IQueryable<T> queryable,
        string tenantId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<ContestDomainOfInfluence> WhereUsesVotingCardsInCurrentContest(
        this IQueryable<ContestDomainOfInfluence> queryable)
    {
        return queryable.Where(x => x.ResponsibleForVotingCards && (x.PoliticalBusinessPermissionEntries!.Any(y => y.Role == PoliticalBusinessRole.Attendee) || (x.Contest!.IsPoliticalAssembly && x.Role != ContestRole.None)));
    }

    public static IQueryable<T> WhereDomainOfInfluenceHasPoliticalBusiness<T>(
        this IQueryable<T> queryable,
        Guid politicalBusinessId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!.Any(pb => pb.PoliticalBusinessId == politicalBusinessId));
    }

    public static IQueryable<T> WhereDomainOfInfluenceHasPoliticalBusinessWithoutExternalPrintingCenter<T>(
        this IQueryable<T> queryable,
        Guid politicalBusinessId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!.Any(pb => pb.PoliticalBusinessId == politicalBusinessId && !pb.DomainOfInfluence!.ExternalPrintingCenter));
    }

    public static IQueryable<T> WhereIsContestManager<T>(this IQueryable<T> queryable, string tenantId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.Contest!.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<T> WhereGenerateVotingCardsTriggered<T>(this IQueryable<T> queryable)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.GenerateVotingCardsTriggered != null);
    }

    public static IQueryable<T> WherePrintJobProcessNotStarted<T>(this IQueryable<T> queryable)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.PrintJob!.State < PrintJobState.ProcessStarted);
    }

    public static IQueryable<T> WhereContestIsApproved<T>(this IQueryable<T> queryable, bool approved = true)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.Contest!.Approved.HasValue == approved);
    }

    public static IQueryable<T> WhereContestIsInTestingPhase<T>(this IQueryable<T> queryable)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.Contest!.State <= ContestState.TestingPhase);
    }

    public static IQueryable<T> WhereContestIsNotLocked<T>(this IQueryable<T> queryable)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.Contest!.State != ContestState.PastLocked && x.DomainOfInfluence.Contest.State != ContestState.Archived);
    }

    public static IQueryable<T> WhereContestIsNotPastPrintingCenterSignUpDeadline<T>(this IQueryable<T> queryable, IClock clock)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x =>
            x.DomainOfInfluence!.Contest!.PrintingCenterSignUpDeadline != null &&
            x.DomainOfInfluence.Contest.PrintingCenterSignUpDeadline.Value > clock.UtcNow);
    }

    public static IQueryable<T> WhereContestIsNotPastGenerateVotingCardsDeadline<T>(this IQueryable<T> queryable, IClock clock)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x =>
            x.DomainOfInfluence!.Contest!.GenerateVotingCardsDeadline != null &&
            x.DomainOfInfluence.Contest.GenerateVotingCardsDeadline.Value > clock.UtcNow);
    }

    public static IQueryable<T> WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast<T>(this IQueryable<T> queryable, IClock clock)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x =>
            x.DomainOfInfluence!.Contest!.PrintingCenterSignUpDeadline == null ||
            x.DomainOfInfluence.Contest.PrintingCenterSignUpDeadline!.Value > clock.UtcNow);
    }

    public static IQueryable<T> WhereHasAccess<T>(
        this IQueryable<T> queryable,
        string tenantId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x =>
            x.DomainOfInfluence!.SecureConnectId == tenantId
            || (x.DomainOfInfluence!.Contest!.DomainOfInfluence!.SecureConnectId == tenantId));
    }

    public static IQueryable<T> WhereHasDomainOfInfluence<T>(
        this IQueryable<T> queryable,
        Guid domainOfInfluenceId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.Id == domainOfInfluenceId);
    }

    public static IQueryable<ContestDomainOfInfluence> WhereCanHaveAttachments(this IQueryable<ContestDomainOfInfluence> queryable)
    {
        return queryable.Where(x => !x.ExternalPrintingCenter && (x.PoliticalBusinessPermissionEntries!.Count > 0 || x.Contest!.IsPoliticalAssembly));
    }

    public static IQueryable<T> WhereIsInContest<T>(this IQueryable<T> queryable, Guid contestId)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => x.DomainOfInfluence!.ContestId == contestId);
    }

    public static IQueryable<ContestDomainOfInfluence> WhereIsInEVotingExport(this IQueryable<ContestDomainOfInfluence> queryable)
    {
        return queryable.Where(ContestDomainOfInfluenceFilterExpressions.InEVotingExportFilter);
    }

    public static IQueryable<T> WhereDomainOfInfluenceIsNotExternalPrintingCenter<T>(this IQueryable<T> queryable)
        where T : IHasContestDomainOfInfluence
    {
        return queryable.Where(x => !x.DomainOfInfluence!.ExternalPrintingCenter);
    }
}
