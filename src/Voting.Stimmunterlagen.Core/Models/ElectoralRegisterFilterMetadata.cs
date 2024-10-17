// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Models;

public record ElectoralRegisterFilterMetadata(
    int NumberOfPersons,
    int NumberOfInvalidPersons,
    bool IsActual,
    DateTime? ActualityDate);
