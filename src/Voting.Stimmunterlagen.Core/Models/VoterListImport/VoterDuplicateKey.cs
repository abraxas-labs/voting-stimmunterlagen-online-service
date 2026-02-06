// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Models.VoterListImport;

public record VoterDuplicateKey(string FirstName, string LastName, string DateOfBirth, string Street, string? HouseNumber, bool ExternalDuplicate);
