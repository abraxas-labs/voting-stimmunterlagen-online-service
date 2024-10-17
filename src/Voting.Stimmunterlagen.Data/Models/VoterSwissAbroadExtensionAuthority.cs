// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterSwissAbroadExtensionAuthority
{
    [ValidateObject]
    public Organisation Organisation { get; set; } = new();

    [MaxLength(60)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(60)]
    public string AddressLine2 { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Town { get; set; } = string.Empty;

    [Range(1000, 9999)]
    public int? SwissZipCode { get; set; }

    [ValidateObject]
    public Country Country { get; set; } = new();
}
