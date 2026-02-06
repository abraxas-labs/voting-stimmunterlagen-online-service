// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterList : BaseEntity, IHasContestDomainOfInfluence
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public VotingCardType VotingCardType { get; set; }

    public int NumberOfVoters { get; set; }

    /// <summary>
    /// Gets or sets the count of voting cards. A voter in a voter list receives
    /// a voting card if <see cref="Voter.VotingCardPrintDisabled"/> is set to false.
    /// </summary>
    public int CountOfVotingCards { get; set; }

    /// <summary>
    /// Gets or sets count of voting cards for householder (excludes duplicates).
    /// </summary>
    public int CountOfVotingCardsForHouseholders { get; set; }

    /// <summary>
    /// Gets or sets count of voting cards with "nicht zustellen" (excludes duplicates).
    /// </summary>
    public int CountOfVotingCardsForDomainOfInfluenceReturnAddress { get; set; }

    /// <summary>
    /// Gets or sets count of voting cards for househoulders which are not "nicht zustellen" (excludes duplicates).
    /// </summary>
    public int CountOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAddress { get; set; }

    public ICollection<Voter>? Voters { get; set; }

    public int Index { get; set; }

    public ICollection<PoliticalBusinessVoterListEntry>? PoliticalBusinessEntries { get; set; }

    public bool? SendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }

    public Guid ImportId { get; set; }

    public VoterListImport? Import { get; set; }
}
