// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Models.Response;

public class CreateVoterDuplicateResponse
{
    public string PersonId { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string DateOfBirth { get; init; } = string.Empty;
}
