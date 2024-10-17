// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class AttachmentCategorySummary
{
    internal AttachmentCategorySummary(
        AttachmentCategory category,
        IReadOnlyCollection<Attachment> attachments,
        int requiredForVoterListsCount)
    {
        Category = category;
        Attachments = attachments;

        foreach (var attachment in attachments)
        {
            TotalOrderedCount += attachment.OrderedCount;
            TotalRequiredCount += attachment.DomainOfInfluenceAttachmentCounts?.Sum(c => c.RequiredCount.GetValueOrDefault())
                ?? attachment.TotalRequiredCount;
        }

        TotalRequiredForVoterListsCount = requiredForVoterListsCount;
    }

    public AttachmentCategory Category { get; }

    public int TotalOrderedCount { get; }

    public int TotalRequiredCount { get; }

    public int TotalRequiredForVoterListsCount { get; }

    public IReadOnlyCollection<Attachment> Attachments { get; }
}
