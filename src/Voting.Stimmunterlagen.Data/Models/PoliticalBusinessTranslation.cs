// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class PoliticalBusinessTranslation : TranslationEntity
{
    public string OfficialDescription { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public Guid PoliticalBusinessId { get; set; }

    public PoliticalBusiness? PoliticalBusiness { get; set; }
}
