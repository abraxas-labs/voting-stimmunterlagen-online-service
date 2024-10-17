// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

/// <summary>
/// A voter duplicate is a voter which exists multiple times (per key-criteria such as <see cref="PersonId"/>) in the same voter list.
/// </summary>
public class VoterDuplicate : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PersonId { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public SexType Sex { get; set; }

    public Guid ListId { get; set; }

    public VoterList? List { get; set; }
}
