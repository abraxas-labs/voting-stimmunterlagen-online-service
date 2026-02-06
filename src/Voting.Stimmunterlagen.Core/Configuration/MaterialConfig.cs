// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.Core.Models.Invoice;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class MaterialConfig
{
    public string Number { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OrderPosition { get; set; } = string.Empty;

    public MaterialCategory Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a comment on the additional invoice position is required or not.
    /// If set to false then comments are disabled.
    /// </summary>
    public bool CommentRequired { get; set; }

    public int? Stations { get; set; }

    /// <summary>
    /// Gets or sets, whether the material is only available for a specific delivery format.
    /// If not set the delivery format does not matter for this material.
    /// </summary>
    public AttachmentFormat? AttachmentFormat { get; set; }

    public MaterialContestType ContestType { get; set; }

    public MaterialVotingCardFormat VotingCardFormat { get; set; }

    /// <summary>
    /// Gets or sets, whether the material is only available for specific shipping methods.
    /// If not set the shipping method does not matter for this material.
    /// </summary>
    public List<VotingCardShippingMethod> VotingCardShippingMethods { get; set; } = new();

    public bool? IsDuplex { get; set; }
}
