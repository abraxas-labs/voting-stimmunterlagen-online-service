// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class VotingCardPrintFileExportJobQueryableExtensions
{
    public static IQueryable<VotingCardPrintFileExportJob> WhereHasDomainOfInfluence(
        this IQueryable<VotingCardPrintFileExportJob> q,
        Guid domainOfInfluenceId)
    {
        return q.Where(x => x.VotingCardGeneratorJob!.DomainOfInfluenceId == domainOfInfluenceId);
    }

    public static IQueryable<VotingCardPrintFileExportJob> WhereContestInTestingPhase(
        this IQueryable<VotingCardPrintFileExportJob> q)
    {
        return q.Where(x => x.VotingCardGeneratorJob!.DomainOfInfluence!.Contest!.State == ContestState.TestingPhase);
    }

    public static IQueryable<VotingCardPrintFileExportJob> WhereIsContestManager(this IQueryable<VotingCardPrintFileExportJob> queryable, string tenantId)
    {
        return queryable.Where(x => x.VotingCardGeneratorJob!.DomainOfInfluence!.Contest!.DomainOfInfluence!.SecureConnectId == tenantId);
    }

    public static IQueryable<VotingCardPrintFileExportJob> WhereDomainOfInfluenceIsNotExternalPrintingCenter(this IQueryable<VotingCardPrintFileExportJob> queryable)
    {
        return queryable.Where(x => !x.VotingCardGeneratorJob!.DomainOfInfluence!.ExternalPrintingCenter);
    }
}
