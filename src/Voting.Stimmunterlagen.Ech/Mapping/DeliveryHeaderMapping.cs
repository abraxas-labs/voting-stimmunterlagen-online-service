// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0058_5_0;

namespace Voting.Stimmunterlagen.Ech.Mapping;

public static class DeliveryHeaderMapping
{
    private const string ElectoralRegisterManufacturer = "Abraxas Informatik AG";
    private const string ElectoralRegisterProduct = "Voting.Stimmregister";

    public static bool IsFromElectoralRegister(HeaderType? deliveryHeader)
    {
        if (deliveryHeader == null || deliveryHeader.SendingApplication == null)
        {
            throw new ArgumentException("Ech0045 does not provide a delivery header with a sending application");
        }

        return deliveryHeader.SendingApplication.Manufacturer.Equals(ElectoralRegisterManufacturer, StringComparison.Ordinal)
            && deliveryHeader.SendingApplication.Product.Equals(ElectoralRegisterProduct, StringComparison.Ordinal);
    }
}
