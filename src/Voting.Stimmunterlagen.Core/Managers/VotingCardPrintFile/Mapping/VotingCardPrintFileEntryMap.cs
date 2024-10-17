// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using CsvHelper.Configuration;
using Voting.Stimmunterlagen.Core.Models.VotingCardPrintFile;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Mapping;

public class VotingCardPrintFileEntryMap : ClassMap<VotingCardPrintFileEntry>
{
    public VotingCardPrintFileEntryMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.IsDuplexPrint).Convert(convertToStringFunction: m => m.Value.IsDuplexPrint ? "D" : "S");
        Map(m => m.PrintPP).Convert(convertToStringFunction: m => m.Value.PrintPP ? "J" : "N");
    }
}
