// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Ech0007_6_0;
using Ech0011_9_0;
using Ech0044_4_1;
using Ech0045_6_0;
using DataModels = Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping.V6;

internal static class VoterMapping
{
    public static VotingPersonType ToEchVoter(
        this DataModels.Voter voter,
        DataModels.VotingCardType votingCardType,
        Dictionary<Guid, List<DataModels.ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        return new VotingPersonType
        {
            Person = GetEchPerson(voter),
            ElectoralAddress = GetEchElectoralAddress(voter),
            IsEvoter = votingCardType == DataModels.VotingCardType.EVoting,
            DomainOfInfluenceInfo = voter.List!.DomainOfInfluence!.ToEchDomainOfInfluenceInfo(doiHierarchyByDoiId),
        };
    }

    private static ElectoralAddressType GetEchElectoralAddress(DataModels.Voter voter)
    {
        return new()
        {
            Person = new()
            {
                MrMrs = voter.Salutation == DataModels.Salutation.Unspecified ? null : voter.Salutation.ToEchMrMrsType(),
                Title = voter.Title,
                FirstName = voter.AddressFirstName,
                LastName = voter.AddressLastName,
            },
            AddressInformation = new()
            {
                AddressLine1 = voter.AddressLine1,
                AddressLine2 = voter.AddressLine2,
                Street = voter.Street,
                HouseNumber = voter.HouseNumber,
                DwellingNumber = voter.DwellingNumber,
                PostOfficeBoxText = voter.PostOfficeBoxText,
                Locality = voter.Locality,
                Town = voter.Town,
                SwissZipCode = (uint?)voter.SwissZipCode,
                ForeignZipCode = voter.ForeignZipCode,
                Country = voter.Country.ToEch0010Country(),
            },
        };
    }

    private static VotingPersonTypePerson GetEchPerson(this DataModels.Voter voter)
    {
        if (voter.VoterType == DataModels.VoterType.SwissAbroad && voter.SwissAbroadPerson == null)
        {
            throw new InvalidOperationException($"Cannot get ech person for swiss abroad voter {voter.Id}, because no swiss abroad person data is null");
        }

        var municipalityId = int.TryParse(voter.Bfs, out var parsedBfs)
            ? parsedBfs == 0 ? null : (int?)parsedBfs
            : null;

        var languageOfCorrespondance = voter.LanguageOfCorrespondence.ToEchLanguage();
        var religionData = voter.Religion != null ? new ReligionDataType { Religion = voter.Religion } : null;
        var personIdentification = new PersonIdentificationType
        {
            LocalPersonId = new NamedPersonIdType
            {
                PersonId = voter.PersonId,
                PersonIdCategory = voter.PersonIdCategory,
            },
            OfficialName = voter.LastName,
            FirstName = voter.FirstName,
            Sex = voter.Sex.ToEchSexType(),
            DateOfBirth = voter.DateOfBirth.ToEchDatePartiallyKnown(),
        };

        var placesOfOrigin = voter.PlacesOfOrigin!
            .Select(x => new PlaceOfOriginType
            {
                Canton = x.Canton.ToEchCantonAbbreviation(),
                OriginName = x.Name,
            })
            .ToList();
        var swissPerson = new SwissPersonType
        {
            PersonIdentification = personIdentification,
            LanguageOfCorrespondance = languageOfCorrespondance,
            PlaceOfOrigin = placesOfOrigin,
            ReligionData = religionData,
        };
        var swissMunicipality = new SwissMunicipalityType
        {
            MunicipalityId = municipalityId,
            MunicipalityName = voter.MunicipalityName,
        };

        var nationality = voter.VoterType switch
        {
            DataModels.VoterType.Swiss => new VotingPersonTypePerson
            {
                Swiss = new SwissDomesticType
                {
                    SwissDomesticPerson = swissPerson,
                    Municipality = swissMunicipality,
                },
            },
            DataModels.VoterType.SwissAbroad => new VotingPersonTypePerson
            {
                SwissAbroad = new SwissAbroadType
                {
                    SwissAbroadPerson = swissPerson,
                    DateOfRegistration = voter.SwissAbroadPerson!.DateOfRegistration,
                    ResidenceCountry = voter.SwissAbroadPerson.ResidenceCountry.ToEch0008Country(),
                    Municipality = swissMunicipality,
                },
            },
            DataModels.VoterType.Foreigner => new VotingPersonTypePerson
            {
                Foreigner = new ForeignerType
                {
                    ForeignerPerson = new ForeignerPersonType
                    {
                        PersonIdentification = personIdentification,
                        LanguageOfCorrespondance = languageOfCorrespondance,
                        ReligionData = religionData,
                    },
                    Municipality = swissMunicipality,
                },
            },
            _ => throw new InvalidOperationException($"Voter with id {voter.Id} has an invalid voter type {voter.VoterType}"),
        };

        if (nationality.SwissAbroad != null)
        {
            nationality.SwissAbroad.SwissAbroadPerson.Extension = voter.SwissAbroadPerson!.Extension?.ToEchSwissPersonExtension();
        }

        return nationality;
    }
}
