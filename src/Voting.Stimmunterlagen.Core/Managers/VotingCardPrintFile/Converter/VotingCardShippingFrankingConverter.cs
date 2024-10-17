// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile.Converter;

public class VotingCardShippingFrankingConverter : ITypeConverter
{
    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return null;
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is not VotingCardShippingFranking castedValue)
        {
            return null;
        }

        return castedValue switch
        {
            VotingCardShippingFranking.B1 => "B1",
            VotingCardShippingFranking.B2 => "B2",
            VotingCardShippingFranking.A => "A",
            VotingCardShippingFranking.GasA => "A",
            VotingCardShippingFranking.GasB => "B",
            VotingCardShippingFranking.WithoutFranking => "F",
            _ => null,
        };
    }
}
