// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class DomainOfInfluenceAttachmentCategorySummariesEntry
{
    public DomainOfInfluenceAttachmentCategorySummariesEntry(
        ContestDomainOfInfluence domainOfInfluence,
        IReadOnlyCollection<AttachmentCategorySummary> attachmentCategorySummaries,
        IReadOnlyCollection<PoliticalBusiness> politicalBusinesses)
    {
        DomainOfInfluence = domainOfInfluence;
        AttachmentCategorySummaries = attachmentCategorySummaries;
        PoliticalBusinesses = politicalBusinesses;
    }

    public ContestDomainOfInfluence DomainOfInfluence { get; }

    public IReadOnlyCollection<AttachmentCategorySummary> AttachmentCategorySummaries { get; }

    public IReadOnlyCollection<PoliticalBusiness> PoliticalBusinesses { get; }
}
