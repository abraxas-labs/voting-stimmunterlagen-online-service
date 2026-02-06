// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Models.Response;

public class VoterDuplicateResponse
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string DateOfBirth { get; init; } = string.Empty;

    public string Street { get; init; } = string.Empty;

    public string? HouseNumber { get; set; }

    public bool ExternalDuplicate { get; init; }
}
