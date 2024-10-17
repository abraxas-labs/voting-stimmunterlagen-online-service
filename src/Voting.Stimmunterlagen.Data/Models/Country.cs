// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.Stimmunterlagen.Data.Models;

public class Country
{
    [MinLength(2)]
    [MaxLength(2)]
    public string? Iso2 { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
