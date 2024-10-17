// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.ValidationAttributes;
using Xunit;

namespace Voting.Stimmunterlagen.Test.Validators.Attributes;

public class ValidEnumValueAttributeTest
{
    [Theory]
    [InlineData(true, VotingCardType.Swiss)]
    [InlineData(true, VotingCardType.Unspecified)]
    [InlineData(false, VotingCardType.Unspecified, VotingCardType.Unspecified)]
    [InlineData(false, VotingCardType.Swiss, VotingCardType.Unspecified, VotingCardType.Swiss)]
    [InlineData(true, VotingCardType.SwissAbroad, VotingCardType.Unspecified, VotingCardType.Swiss)]
    public void ShouldWork(bool valid, object value, params object[] forbiddenValues)
    {
        var attr = new ValidEnumValueAttribute(forbiddenValues);
        attr.IsValid(value)
            .Should()
            .Be(valid);
    }

    [Fact]
    public void ShouldNotBeValidIfNotDefined()
    {
        var attr = new ValidEnumValueAttribute();
        attr.IsValid((VotingCardType)int.MaxValue)
            .Should()
            .BeFalse();
    }
}
