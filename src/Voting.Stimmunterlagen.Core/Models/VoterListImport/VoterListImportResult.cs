// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using DataVoterListImport = Voting.Stimmunterlagen.Data.Models.VoterListImport;

namespace Voting.Stimmunterlagen.Core.Models.VoterListImport;

public class VoterListImportResult
{
    public DataVoterListImport Import { get; init; } = null!;

    public List<VoterDuplicateKey> VoterDuplicates { get; init; } = new();

    public int VoterDuplicatesCount { get; init; }

    public bool Success { get; init; }
}
