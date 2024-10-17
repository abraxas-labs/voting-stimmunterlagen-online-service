// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.EVoting.Models;

public class DomainOfInfluenceVotingCardPrintData
{
    public VotingCardShippingFranking ShippingAway { get; set; }

    public VotingCardShippingFranking ShippingReturn { get; set; }

    public VotingCardShippingMethod ShippingMethod { get; set; }
}
