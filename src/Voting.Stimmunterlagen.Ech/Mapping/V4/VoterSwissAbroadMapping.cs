// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0045_4_0;
using Voting.Lib.Ech.Ech0045_4_0.Models;
using VoterSwissAbroadExtension = Voting.Stimmunterlagen.Data.Models.VoterSwissAbroadExtension;
using VoterSwissAbroadPerson = Voting.Stimmunterlagen.Data.Models.VoterSwissAbroadPerson;

namespace Voting.Stimmunterlagen.Ech.Mapping.V4;

internal static class VoterSwissAbroadMapping
{
    public static VoterSwissAbroadPerson ToSwissAbroadPerson(this SwissAbroadType swissAbroad, SwissPersonExtension? personExtension)
    {
        return new()
        {
            DateOfRegistration = swissAbroad.DateOfRegistration,
            ResidenceCountry = swissAbroad.ResidenceCountry.ToCountry(),
            Extension = personExtension?.ToSwissAbroadExtension(),
        };
    }

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

    private static VoterSwissAbroadExtension ToSwissAbroadExtension(this SwissPersonExtension swissPersonExtension)
    {
        if (swissPersonExtension == null)
        {
            throw new ArgumentException("Swiss person extension is missing");
        }

        var extension = new VoterSwissAbroadExtension
        {
            PostageCode = swissPersonExtension.PostageCode ?? "?",
        };

        if (swissPersonExtension.Address != null)
        {
            extension.Address = new()
            {
                Line1 = swissPersonExtension.Address.Line1,
                Line2 = swissPersonExtension.Address.Line2,
                Line3 = swissPersonExtension.Address.Line3,
                Line4 = swissPersonExtension.Address.Line4,
                Line5 = swissPersonExtension.Address.Line5,
                Line6 = swissPersonExtension.Address.Line6,
                Line7 = swissPersonExtension.Address.Line7,
            };
        }

        if (swissPersonExtension.VotingPlace != null)
        {
            extension.Authority = new()
            {
                Organisation = new()
                {
                    Name = swissPersonExtension.VotingPlace.Organisation.OrganisationName,
                    AddOn1 = swissPersonExtension.VotingPlace.Organisation.OrganisationNameAddOn1,
                },
                AddressLine1 = swissPersonExtension.VotingPlace.AddressInformation.AddressLine1,
                AddressLine2 = swissPersonExtension.VotingPlace.AddressInformation.AddressLine2,
                Country = swissPersonExtension.VotingPlace.AddressInformation.Country.ToCountry(),
                Street = swissPersonExtension.VotingPlace.AddressInformation.Street,
                SwissZipCode = (int?)swissPersonExtension.VotingPlace.AddressInformation.SwissZipCode,
                Town = swissPersonExtension.VotingPlace.AddressInformation.Town,
            };
        }

        return extension;
    }
}
