// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Ech0155_4_0;
using Voting.Stimmunterlagen.Data.Models;
using ContestType = Ech0045_4_0.ContestType;

namespace Voting.Stimmunterlagen.Ech.Mapping.V4;

internal static class ContestMapping
{
    internal static ContestType ToEchContestType(this Contest contest)
    {
        var contestDescriptionInfos = contest.Translations!
            .OrderBy(t => t.Language)
            .Select(t => new ContestDescriptionInformationTypeContestDescriptionInfo
            {
                Language = t.Language,
                ContestDescription = t.Description,
            })
            .ToList();

        return new ContestType
        {
            ContestDate = contest.Date,
            ContestDescription = contestDescriptionInfos,
        };
    }
}
