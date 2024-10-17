// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class CantonSettings : BaseEntity
{
    public DomainOfInfluenceCanton Canton { get; set; }

    public string VotingDocumentsEVotingEaiMessageType { get; set; } = string.Empty;
}
