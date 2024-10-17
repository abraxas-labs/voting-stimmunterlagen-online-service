// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterPlaceOfOrigin
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [ValidEnumValue]
    public CantonAbbreviation Canton { get; set; }

    public Guid VoterId { get; set; }

    public Voter? Voter { get; set; }
}
