// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestDomainOfInfluence : BaseDomainOfInfluence<ContestDomainOfInfluence, ContestDomainOfInfluenceHierarchyEntry>,
    IHasContest
{
    public Contest? Contest { get; set; }

    public Guid ContestId { get; set; }

    // The id of the VOTING Basis DomainOfInfluence (the id of the "live" doi)
    // don't use reference integrity here since we want to preserve this id
    public Guid BasisDomainOfInfluenceId { get; set; }

    public DateTime? GenerateVotingCardsTriggered { get; set; }

    public ICollection<PoliticalBusiness>? PoliticalBusinesses { get; set; }

    public ICollection<ContestDomainOfInfluenceCountingCircle>? CountingCircles { get; set; }

    public ContestRole Role { get; set; }

    public ICollection<StepState>? StepStates { get; set; }

    public ICollection<PoliticalBusinessPermissionEntry>? PoliticalBusinessPermissionEntries { get; set; }

    public Contest? ManagedContest { get; set; }

    public ICollection<Attachment>? Attachments { get; set; }

    public ICollection<VoterList>? VoterLists { get; set; }

    public ICollection<VoterListImport>? VoterListImports { get; set; }

    public ICollection<DomainOfInfluenceVotingCardLayout>? VotingCardLayouts { get; set; }

    public ICollection<DomainOfInfluenceAttachmentCount>? DomainOfInfluenceAttachmentCounts { get; set; }

    public ICollection<VotingCardGeneratorJob>? VotingCardGeneratorJobs { get; set; }

    public ICollection<DomainOfInfluenceVoterDuplicate>? VoterDuplicates { get; set; }

    public DomainOfInfluenceVotingCardConfiguration? VotingCardConfiguration { get; set; }

    public PrintJob? PrintJob { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manual voter list upload
    /// is allowed even if <see cref="DomainOfInfluenceCantonDefaults.ElectoralRegistrationEnabled"/> is <c>true</c>.
    /// </summary>
    public bool AllowManualVoterListUpload { get; set; }

    /// <summary>
    /// Gets a value indicating whether manual voter list uploads can be performed for this DOI.
    /// </summary>
    public bool CanManuallyUploadVoterList => !ElectoralRegistrationEnabled || AllowManualVoterListUpload;

    /// <summary>
    /// Gets or sets a value indicating when the last voter list was imported or updated or when the last manual voting card was generated.
    /// </summary>
    public DateTime? LastVoterUpdate { get; set; }

    public int CountOfEmptyVotingCards { get; set; }

    public DateTime? LastCountOfEmptyVotingCardsUpdate { get; set; }

    public ICollection<AdditionalInvoicePosition>? AdditionalInvoicePositions { get; set; }

    public bool UsesVotingCardsInCurrentContest()
    {
        return ResponsibleForVotingCards && (PoliticalBusinessPermissionEntries!.Any(x => x.Role == PoliticalBusinessRole.Attendee) || (Contest!.IsPoliticalAssembly && Role != ContestRole.None));
    }
}
