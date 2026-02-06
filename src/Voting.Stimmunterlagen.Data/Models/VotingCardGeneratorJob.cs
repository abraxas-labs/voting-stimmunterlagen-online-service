// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class VotingCardGeneratorJob : BaseEntity, IHasContestDomainOfInfluence
{
    public string FileName { get; set; } = string.Empty;

    public int CountOfVoters { get; set; }

    public DateTime? Started { get; set; }

    public DateTime? Completed { get; set; }

    public DateTime? Failed { get; set; }

    public VotingCardGeneratorJobState State { get; set; } = VotingCardGeneratorJobState.Ready;

    public ICollection<Voter> Voter { get; set; } = new HashSet<Voter>();

    public bool HasEmptyVotingCards { get; set; }

    public DomainOfInfluenceVotingCardLayout? Layout { get; set; }

    // e-voting jobs run offline and have no layout.
    public Guid? LayoutId { get; set; }

    public string Runner { get; set; } = string.Empty;

    public string CallbackToken { get; set; } = string.Empty;

    public int DraftId { get; set; }

    public VotingCardPrintFileExportJob? VotingCardPrintFileExportJob { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }
}
