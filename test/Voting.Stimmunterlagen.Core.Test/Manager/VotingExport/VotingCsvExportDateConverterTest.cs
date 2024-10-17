// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Globalization;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Managers.VotingExport.Converters;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Manager.VotingExport;

public class VotingCsvExportDateConverterTest
{
    private static readonly VotingCsvExportDateConverter Converter = new();

    [Theory]
    [InlineData("2008-11-01T19:35:00.0000000Z", "01.11.2008")]
    [InlineData("2008-01-12T00:00:00.0000000Z", "12.01.2008")]
    [InlineData("2008-11-01T19:35:12.1000000Z", "01.11.2008")]
    [InlineData(null, "")]
    public void Test(string? dateTimeStr, string expected)
    {
        var dateTime = dateTimeStr == null
            ? null
            : (DateTime?)DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture);
        Converter.ConvertToString(dateTime, null!, null!)
            .Should()
            .Be(expected);
    }
}
