// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluenceVotingCardConfiguration : BaseEntity, IHasContestDomainOfInfluence
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public int SampleCount { get; set; }

    public VotingCardGroup[] Groups { get; set; } = Array.Empty<VotingCardGroup>();

    public VotingCardSort[] Sorts { get; set; } = Array.Empty<VotingCardSort>();
}
