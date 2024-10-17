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

    public ICollection<Voter>? Voters { get; set; }

    public int Index { get; set; }

    public ICollection<PoliticalBusinessVoterListEntry>? PoliticalBusinessEntries { get; set; }

    // Anz. Stimmberechtige mit "nicht zustellen".
    public int CountOfSendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }

    public bool? SendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }

    public Guid ImportId { get; set; }

    public VoterListImport? Import { get; set; }

    public bool HasVoterDuplicates { get; set; }

    public ICollection<VoterDuplicate>? VoterDuplicates { get; set; }
}
