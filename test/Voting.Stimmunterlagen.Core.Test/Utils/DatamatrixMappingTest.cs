// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class DatamatrixMappingTest
{
    [Theory]
    [InlineData("", false, VoterType.Swiss, "X")]
    [InlineData("000", false, VoterType.Swiss, "X")]
    [InlineData("111", false, VoterType.Swiss, "E")]
    [InlineData("121", false, VoterType.Swiss, "K")]
    [InlineData("121", true, VoterType.Swiss, "K WR")]
    [InlineData("111", false, VoterType.Foreigner, "E WR")]
    [InlineData("41", false, VoterType.Swiss, "X")]
    public void TestReligionMapping(
        string? religion,
        bool isMinor,
        VoterType voterType,
        string expectedResult)
    {
        var result = DatamatrixMapping.MapReligion(religion, isMinor, voterType);
        result.Should().Be(expectedResult);
    }
}
