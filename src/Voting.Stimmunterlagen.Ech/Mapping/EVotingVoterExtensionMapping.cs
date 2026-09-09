// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using Ech0045_VoterExtension_1_0;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping;

internal static class EVotingVoterExtensionMapping
{
    public static VoterExtensionType ToEchEVotingVoterExtension(this VoterSwissAbroadPerson person)
    {
        return new()
        {
            UnstructuredPhysicalAddress = ToUnstructuredPhysicalAddress(person),
        };
    }

    private static UnstructuredPhysicalAddressType? ToUnstructuredPhysicalAddress(VoterSwissAbroadPerson person)
    {
        var address = person.Extension?.Address;
        var residenceCountry = person.ResidenceCountry.ToCountry();

        if (address is null || residenceCountry is null)
        {
            return null;
        }

        var lines = new[]
        {
            address.Line1,
            address.Line2,
            address.Line3,
            address.Line4,
            address.Line5,
            address.Line6,
            address.Line7,
        }
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToArray();

        if (lines.Length < 2)
        {
            throw new InvalidOperationException("An unstructured physical address requires at least 2 non-empty address lines.");
        }

        // The extension schema does not allow gaps between the address lines.
        return new UnstructuredPhysicalAddressType
        {
            AddressLine1 = lines[0],
            AddressLine2 = lines[1],
            AddressLine3 = lines.ElementAtOrDefault(2),
            AddressLine4 = lines.ElementAtOrDefault(3),
            AddressLine5 = lines.ElementAtOrDefault(4),
            AddressLine6 = lines.ElementAtOrDefault(5),
            AddressLine7 = lines.ElementAtOrDefault(6),
            Country = residenceCountry,
        };
    }

    private static CountryType? ToCountry(this Country country)
    {
        if (country == null || string.IsNullOrEmpty(country.Iso2))
        {
            return null;
        }

        return new()
        {
            CountryIdIso2 = country.Iso2,
            CountryNameShort = country.Name,
        };
    }
}
