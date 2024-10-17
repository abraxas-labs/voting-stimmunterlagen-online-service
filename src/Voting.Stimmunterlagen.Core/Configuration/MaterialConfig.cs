// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Models.Invoice;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class MaterialConfig
{
    public string Number { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OrderPosition { get; set; } = string.Empty;

    public MaterialCategory Category { get; set; }

    public int? Stations { get; set; }

    /// <summary>
    /// Gets or sets, whether the material is only available for a specific delivery format.
    /// If not set the delivery format does not matter for this material.
    /// </summary>
    public AttachmentFormat? AttachmentFormat { get; set; }
}
