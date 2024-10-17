// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;

namespace Voting.Stimmunterlagen.Data.Models;

public class Organisation
{
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(60)]
    public string AddOn1 { get; set; } = string.Empty;
}
