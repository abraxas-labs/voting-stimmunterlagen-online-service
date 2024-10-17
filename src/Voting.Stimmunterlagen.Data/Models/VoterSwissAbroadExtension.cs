// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterSwissAbroadExtension
{
    [Required(AllowEmptyStrings = false)]
    [MaxLength(50)]
    public string PostageCode { get; set; } = string.Empty;

    [ValidateObject]
    public VoterSwissAbroadExtensionAuthority? Authority { get; set; }

    [ValidateObject]
    public VoterSwissAbroadExtensionAddress? Address { get; set; }
}
