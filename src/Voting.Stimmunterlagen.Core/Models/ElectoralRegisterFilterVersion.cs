// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Models;

public record ElectoralRegisterFilterVersion(
    Guid Id,
    string Name,
    DateOnly Deadline,
    int NumberOfPersons,
    int NumberOfInvalidPersons,
    DateTime CreatedAt,
    string CreatedByName);
