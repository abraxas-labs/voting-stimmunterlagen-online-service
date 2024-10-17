// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Models.Response;

public class CreateUpdateVoterListResponse
{
    public Guid Id { get; set; }

    public int VotingCardType { get; init; }

    public int NumberOfVoters { get; init; }

    public List<CreateVoterDuplicateResponse> VoterDuplicates { get; init; } = new();
}
