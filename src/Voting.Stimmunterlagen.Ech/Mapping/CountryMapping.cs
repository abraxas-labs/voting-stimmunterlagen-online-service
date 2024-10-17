// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping;

internal static class CountryMapping
{
    public static Country ToCountry(this Ech0010_6_0.CountryType country) => new()
    {
        Iso2 = country.CountryIdIso2,
        Name = country.CountryNameShort,
    };

    public static Country ToCountry(this Ech0008_3_0.CountryType country) => new()
    {
        Iso2 = country.CountryIdIso2,
        Name = country.CountryNameShort,
    };

    public static Ech0010_6_0.CountryType ToEch0010Country(this Country country) => new Ech0010_6_0.CountryType
    {
        CountryIdIso2 = country.Iso2,
        CountryNameShort = country.Name,
    };

    public static Ech0008_3_0.CountryType ToEch0008Country(this Country country) => new Ech0008_3_0.CountryType
    {
        CountryIdIso2 = country.Iso2,
        CountryNameShort = country.Name,
    };
}
