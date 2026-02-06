// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public abstract class VotingCardLayout : BaseEntity
{
    public bool AllowCustom { get; set; }

    public int? TemplateId { get; set; }

    public Template? Template { get; set; }

    public VotingCardType VotingCardType { get; set; }

    public VotingCardLayoutDataConfiguration DataConfiguration { get; set; } = new();
}
