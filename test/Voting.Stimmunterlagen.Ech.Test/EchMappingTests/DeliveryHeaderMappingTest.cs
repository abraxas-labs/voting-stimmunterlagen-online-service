// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0058_5_0;
using FluentAssertions;
using Voting.Stimmunterlagen.Ech.Mapping;
using Xunit;

namespace Voting.Stimmunterlagen.Ech.Test.EchMappingTests;

public class DeliveryHeaderMappingTest
{
    [Theory]
    [InlineData(null, null, false)]
    [InlineData("", "", false)]
    [InlineData("Abraxas Informatik AG", "Voting.Stimmregister", true)]
    [InlineData("Abraxas Informatik AG", "Voting.stimmregister", false)]
    [InlineData("innosolv AG", "innosolvcity", true)]
    [InlineData("innosolv AG", "", false)]
    [InlineData("", "Voting.Stimmregister", false)]
    public void TestIsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(string? manufacturer, string? product, bool expectedResult)
    {
        var result = DeliveryHeaderMapping.IsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(BuildDeliveryHeader(manufacturer, product));
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void TestIsFromAutoSplitAppWithMissingSendingApplicationShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => DeliveryHeaderMapping.IsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(new HeaderType()));
    }

    private HeaderType BuildDeliveryHeader(string? manufacturer, string? product)
    {
        return new()
        {
            SendingApplication = new()
            {
                Manufacturer = manufacturer,
                Product = product,
            },
        };
    }
}
