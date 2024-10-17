// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Data.Extensions;

namespace Voting.Stimmunterlagen.Data.Models;

public class Contest : BaseEntity
{
    public DateTime Date { get; set; }

    public Guid? DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public ContestState State { get; set; } = ContestState.TestingPhase;

    public ICollection<ContestDomainOfInfluence>? ContestDomainOfInfluences { get; set; }

    public ICollection<ContestCountingCircle>? ContestCountingCircles { get; set; }

    public ICollection<PoliticalBusiness>? PoliticalBusinesses { get; set; }

    public bool IsSingleAttendeeContest { get; set; }

    public bool EVoting { get; set; }

    public DateTime? Approved { get; set; }

    public bool IsApproved => Approved.HasValue;

    public DateTime? PrintingCenterSignUpDeadline { get; set; }

    public DateTime? AttachmentDeliveryDeadline { get; set; }

    public DateTime? GenerateVotingCardsDeadline { get; set; }

    public ICollection<ContestVotingCardLayout>? VotingCardLayouts { get; set; }

    public bool TestingPhaseEnded => State.TestingPhaseEnded();

    public ICollection<ContestTranslation>? Translations { get; set; }

    public ContestEVotingExportJob? EVotingExportJob { get; set; }

    public ICollection<Voter>? Voters { get; set; }

    public int OrderNumber { get; set; }

    public bool IsPoliticalAssembly { get; set; }

    public string Description => Translations.GetTranslated(x => x.Description);
}
