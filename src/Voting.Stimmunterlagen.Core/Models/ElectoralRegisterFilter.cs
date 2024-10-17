// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Models;

public record ElectoralRegisterFilter(Guid Id, string Name, string Description);
