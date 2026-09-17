// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Snapper;
using Voting.Stimmunterlagen.Core.Utils;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class DummyDoiBuilderTest
{
    [Fact]
    public void GetDummyDataShouldWork()
    {
        var doi = DummyDoiBuilder.GetDummyDomainOfInfluence("123456789", new() { ShippingAway = Data.Models.VotingCardShippingFranking.GasA, ShippingMethod = Data.Models.VotingCardShippingMethod.PrintingPackagingShippingToCitizen, ShippingReturn = Data.Models.VotingCardShippingFranking.B1, ShippingVotingCardsToDeliveryAddress = true }, Data.Models.VotingCardColor.Blue);
        doi.ShouldMatchSnapshot();
    }
}
