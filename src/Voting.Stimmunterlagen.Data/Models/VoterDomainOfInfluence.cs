// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterDomainOfInfluence : BaseEntity
{
    [Required]
    [ValidEnumValue]
    public DomainOfInfluenceType DomainOfInfluenceType { get; set; }

    [Required]
    [MaxLength(50)]
    public string DomainOfInfluenceIdentification { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DomainOfInfluenceName { get; set; }

    public Guid VoterId { get; set; }

    public Voter Voter { get; set; } = null!;
}
