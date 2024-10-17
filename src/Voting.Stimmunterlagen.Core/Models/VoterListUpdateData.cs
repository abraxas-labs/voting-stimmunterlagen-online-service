// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Core.Models;

public record VoterListUpdateData(
    Guid Id,
    IReadOnlyCollection<Guid> PoliticalBusinessIds,
    bool? SendVotingCardsToDomainOfInfluenceReturnAddress);
