// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestVotingCardLayout : VotingCardLayout, IHasContest
{
    public Contest? Contest { get; set; }

    public Guid ContestId { get; set; }
}
