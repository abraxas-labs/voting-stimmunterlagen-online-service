// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Ech.Ech0045_6_0.Models;
using VoterSwissAbroadExtension = Voting.Stimmunterlagen.Data.Models.VoterSwissAbroadExtension;

namespace Voting.Stimmunterlagen.Ech.Mapping.V6;

internal static class VoterSwissAbroadMapping
{
    public static SwissPersonExtension ToEchSwissPersonExtension(this VoterSwissAbroadExtension extension)
    {
        return new()
        {
            PostageCode = extension.PostageCode,
            VotingPlace = extension.Authority != null
                ? new()
                {
                    Organisation = new()
                    {
                        OrganisationName = extension.Authority.Organisation.Name,
                        OrganisationNameAddOn1 = extension.Authority.Organisation.AddOn1,
                    },
                    AddressInformation = new()
                    {
                        AddressLine1 = extension.Authority.AddressLine1,
                        AddressLine2 = extension.Authority.AddressLine2,
                        Street = extension.Authority.Street,
                        Town = extension.Authority.Town,
                        SwissZipCode = (uint?)extension.Authority.SwissZipCode,
                        Country = extension.Authority.Country.ToEch0010Country(),
                    },
                }
                : null,
            Address = extension.Address != null
                ? new()
                {
                    Line1 = extension.Address.Line1,
                    Line2 = extension.Address.Line2,
                    Line3 = extension.Address.Line3,
                    Line4 = extension.Address.Line4,
                    Line5 = extension.Address.Line5,
                    Line6 = extension.Address.Line6,
                    Line7 = extension.Address.Line7,
                }
                : null,
        };
    }
}
