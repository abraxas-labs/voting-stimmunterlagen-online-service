// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Ech0058_5_0;

namespace Voting.Stimmunterlagen.Ech.Mapping;

public static class DeliveryHeaderMapping
{
    private static readonly AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp[] AutoSplitApplications =
    [
        new("Abraxas Informatik AG", "Voting.Stimmregister"),
        new("innosolv AG", "innosolvcity"),
    ];

    public static bool IsFromAutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(HeaderType? deliveryHeader)
    {
        if (deliveryHeader == null || deliveryHeader.SendingApplication == null)
        {
            throw new ArgumentException("Ech0045 does not provide a delivery header with a sending application");
        }

        return AutoSplitApplications.Any(a => a.Manufacturer.Equals(deliveryHeader.SendingApplication.Manufacturer, StringComparison.Ordinal)
            && a.Product.Equals(deliveryHeader.SendingApplication.Product, StringComparison.Ordinal));
    }

    private sealed record AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplitApp(string Manufacturer, string Product);
}
