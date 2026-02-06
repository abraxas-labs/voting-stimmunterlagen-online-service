// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.QueryableExtensions;

public static class AttachmentQueryableExtensions
{
    /// <summary>
    /// Filters only attachments, which have a domain of influence count or the current tenant owns,
    /// or if a child contains at least one political business entry which match any <see cref="managedPoliticalBusinessIds"/>.
    /// </summary>
    /// <param name="queryable">The queryable.</param>
    /// <param name="tenantId">Tenant id of the current user.</param>
    /// <param name="managedDomainOfInfluenceId">Domain of influence id which is managed by the current user.</param>
    /// <param name="managedPoliticalBusinessIds">Political business ids which are managed by the corresponding <see cref="managedDomainOfInfluenceId"/>.</param>
    /// <returns>A filtered queryable.</returns>
    public static IQueryable<Attachment> WhereHasAccess(this IQueryable<Attachment> queryable, string tenantId, Guid managedDomainOfInfluenceId, List<Guid> managedPoliticalBusinessIds)
    {
        return queryable
            .Where(a => (a.DomainOfInfluenceId == managedDomainOfInfluenceId && a.DomainOfInfluence!.SecureConnectId == tenantId)
                        || a.DomainOfInfluenceAttachmentCounts!.Any(x => x.DomainOfInfluenceId == managedDomainOfInfluenceId && x.DomainOfInfluence!.SecureConnectId == tenantId)
                        || (a.PoliticalBusinessEntries!.Any(x => managedPoliticalBusinessIds.Contains(x.PoliticalBusinessId)) && a.DomainOfInfluence!.HierarchyEntries!.Any(x => x.ParentDomainOfInfluenceId == managedDomainOfInfluenceId && x.ParentDomainOfInfluence!.SecureConnectId == tenantId)));
    }

    public static IQueryable<Attachment> WhereHasAccess(this IQueryable<Attachment> queryable, string tenantId, Guid contestId)
    {
        return queryable
            .Where(a => a.DomainOfInfluence!.ContestId == contestId
                    && (a.DomainOfInfluence!.SecureConnectId == tenantId
                        || a.DomainOfInfluenceAttachmentCounts!.Any(x => x.DomainOfInfluence!.SecureConnectId == tenantId)
                        || a.PoliticalBusinessEntries.Any(x => x.PoliticalBusiness!.DomainOfInfluence!.SecureConnectId == tenantId)));
    }

    public static IQueryable<Attachment> WhereCanSetDomainOfInfluenceCount(this IQueryable<Attachment> queryable, string tenantId, Guid domainOfInfluenceId)
    {
        return queryable
            .Where(a => a.DomainOfInfluenceAttachmentCounts!.Any(c => c.DomainOfInfluenceId == domainOfInfluenceId && c.DomainOfInfluence!.SecureConnectId == tenantId));
    }
}
