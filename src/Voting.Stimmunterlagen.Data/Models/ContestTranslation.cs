// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestTranslation : TranslationEntity
{
    public string Description { get; set; } = string.Empty;

    public Guid ContestId { get; set; }

    public Contest? Contest { get; set; }
}
