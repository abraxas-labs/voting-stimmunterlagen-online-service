// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Voting.Stimmunterlagen.Data.ValidationAttributes;

namespace Voting.Stimmunterlagen.Models.Request;

public class UpdateVoterListImportRequest
{
    [Required]
    [Past]
    public DateTime LastUpdate { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;
}
