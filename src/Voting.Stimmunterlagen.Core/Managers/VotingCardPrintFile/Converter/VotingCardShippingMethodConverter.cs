// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Converter;

public class VotingCardShippingMethodConverter : ITypeConverter
{
    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return null;
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is not VotingCardShippingMethod castedValue)
        {
            return null;
        }

        return castedValue switch
        {
            VotingCardShippingMethod.PrintingPackagingShippingToCitizen => "A",
            VotingCardShippingMethod.PrintingPackagingShippingToMunicipality => "B",
            VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality => "C",
            _ => null,
        };
    }
}
