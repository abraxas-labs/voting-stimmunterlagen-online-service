// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Data.Extensions;

namespace Voting.Stimmunterlagen.Data.Models;

public class PoliticalBusiness : BaseEntity, IHasContest, IHasContestDomainOfInfluence
{
    public string PoliticalBusinessNumber { get; set; } = string.Empty;

    public bool Active { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid ContestId { get; set; }

    public Contest? Contest { get; set; }

    public PoliticalBusinessType PoliticalBusinessType { get; set; }

    public ICollection<PoliticalBusinessPermissionEntry>? PermissionEntries { get; set; }

    public bool Approved { get; set; }

    public ICollection<PoliticalBusinessAttachmentEntry>? AttachmentEntries { get; set; }

    public ICollection<PoliticalBusinessVoterListEntry>? VoterListEntries { get; set; }

    public ICollection<PoliticalBusinessTranslation>? Translations { get; set; }

    public string OfficialDescription => Translations.GetTranslated(x => x.OfficialDescription);

    public string ShortDescription => Translations.GetTranslated(x => x.ShortDescription);
}
