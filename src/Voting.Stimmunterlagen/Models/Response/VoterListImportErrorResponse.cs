// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Models.Response;

public class VoterListImportErrorResponse
{
    public List<VoterDuplicateResponse> VoterDuplicates { get; init; } = new();

    public int VoterDuplicatesCount { get; init; }
}
