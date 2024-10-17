// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Models.Response;

public class CreateUpdateVoterListImportResponse
{
    public Guid ImportId { get; set; }

    public List<CreateUpdateVoterListResponse>? VoterLists { get; init; }

    public bool AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit { get; set; }
}
