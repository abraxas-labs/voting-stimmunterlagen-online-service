// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class Attachment : BaseEntity, IHasContestDomainOfInfluence
{
    public string Name { get; set; } = string.Empty;

    public AttachmentCategory Category { get; set; }

    public AttachmentFormat Format { get; set; }

    public string Color { get; set; } = string.Empty;

    public string Supplier { get; set; } = string.Empty;

    public DateTime DeliveryPlannedOn { get; set; }

    public DateTime? DeliveryReceivedOn { get; set; }

    public ICollection<PoliticalBusinessAttachmentEntry> PoliticalBusinessEntries { get; set; }
        = new HashSet<PoliticalBusinessAttachmentEntry>();

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public ICollection<DomainOfInfluenceAttachmentCount>? DomainOfInfluenceAttachmentCounts { get; set; }

    public AttachmentState State { get; set; } = AttachmentState.Defined;

    /// <summary>
    /// Gets or sets count which is ordered by the manager and which will be delivered to the D&V.
    /// "Bestellt".
    /// </summary>
    public int OrderedCount { get; set; }

    /// <summary>
    /// Gets or sets sum of counts which are required by all attendee dois, which are manually entered by the attendee.
    /// "Benötigt".
    /// </summary>
    public int TotalRequiredCount { get; set; }

    /// <summary>
    /// Gets or sets count which is required by the imported voter lists of all dois for this attachment.
    /// "gem. Stimmregister".
    /// </summary>
    public int TotalRequiredForVoterListsCount { get; set; }

    public int? Station { get; set; }

    public ICollection<AttachmentComment> Comments { get; set; }
        = new HashSet<AttachmentComment>();
}
