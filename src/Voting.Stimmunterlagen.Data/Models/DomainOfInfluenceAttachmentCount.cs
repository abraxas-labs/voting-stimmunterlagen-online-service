// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluenceAttachmentCount : BaseEntity, IHasContestDomainOfInfluence
{
    public Guid DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid AttachmentId { get; set; }

    public Attachment? Attachment { get; set; }

    /// <summary>
    /// Gets or sets count which is required by the doi. This is manually entered by the attendee.
    /// "Benötigt".
    /// </summary>
    public int? RequiredCount { get; set; }

    /// <summary>
    /// Gets or sets count which is required by the imported voter lists for this attachment.
    /// "Gem. Stimmregister".
    /// </summary>
    public int RequiredForVoterListsCount { get; set; }
}
