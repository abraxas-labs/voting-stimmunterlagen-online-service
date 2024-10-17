// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Ech0007_6_0;
using Ech0010_6_0;
using Ech0011_8_1;
using Ech0044_4_1;
using Ech0045_4_0;
using Voting.Lib.Ech.Ech0045_4_0.Models;
using DataModels = Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping;

internal static class VoterMapping
{
    private const string UnknownBfs = "0000";
    private const string UnknownMunicipalityName = "?";

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

    // if this is adjusted, also test ManualVotingCardGeneratorJobs ("Nachdruck")
    public static DataModels.Voter ToVoter(this VotingPersonType votingPerson, int index, bool shippingVotingCardsToDeliveryAddress, bool eVotingEnabled)
    {
        var address = shippingVotingCardsToDeliveryAddress && votingPerson.DeliveryAddress != null
            ? votingPerson.DeliveryAddress
            : votingPerson.ElectoralAddress;

        var voter = new DataModels.Voter
        {
            Salutation = address.Person.MrMrs.ToSalutation(),
            AddressFirstName = address.Person.FirstName,
            AddressLastName = address.Person.LastName,
            Title = address.Person.Title,
            AddressLine1 = address.AddressInformation.AddressLine1,
            AddressLine2 = address.AddressInformation.AddressLine2,
            Street = address.AddressInformation.Street ?? string.Empty,
            HouseNumber = address.AddressInformation.HouseNumber,
            DwellingNumber = address.AddressInformation.DwellingNumber,
            PostOfficeBoxText = address.AddressInformation.PostOfficeBoxText,
            Locality = address.AddressInformation.Locality,
            Town = address.AddressInformation.Town ?? string.Empty,
            SwissZipCode = (int?)address.AddressInformation.SwissZipCode,
            ForeignZipCode = address.AddressInformation.ForeignZipCode,
            Country = address.AddressInformation.Country.ToCountry(),
            SourceIndex = index,
        };

        MapEchPersonToVoter(votingPerson.Person, voter);
        voter.VotingCardType = GetVotingCardType(votingPerson, voter.SendVotingCardsToDomainOfInfluenceReturnAddress, eVotingEnabled);

        return voter;
    }

    public static DataModels.VotingCardType GetVotingCardType(VotingPersonType votingPerson, bool sendVotingCardsToDomainOfInfluenceReturnAddress, bool eVotingEnabled)
    {
        // "Nicht zustellen" does not exist in E-Voting, we automatically map them to "Swiss" voting cards.
        return votingPerson.IsEvoter == true && !sendVotingCardsToDomainOfInfluenceReturnAddress && eVotingEnabled
            ? DataModels.VotingCardType.EVoting
            : DataModels.VotingCardType.Swiss;
    }

    private static void MapEchPersonToVoter(VotingPersonTypePerson? person, DataModels.Voter voter)
    {
        if (person?.Swiss != null)
        {
            voter.VoterType = DataModels.VoterType.Swiss;
            MapEchPersonToVoter(
                person.Swiss,
                voter,
                x => x.SwissDomesticPerson.PersonIdentification,
                x => x.SwissDomesticPerson.LanguageOfCorrespondance,
                x => x.Municipality,
                x => x.SwissDomesticPerson.PlaceOfOrigin,
                x => x.SwissDomesticPerson.ReligionData,
                x => x.SwissDomesticPerson.Extension);
            return;
        }

        if (person?.SwissAbroad != null)
        {
            voter.VoterType = DataModels.VoterType.SwissAbroad;
            MapEchPersonToVoter(
                person.SwissAbroad,
                voter,
                x => x.SwissAbroadPerson.PersonIdentification,
                x => x.SwissAbroadPerson.LanguageOfCorrespondance,
                x => x.Municipality,
                x => x.SwissAbroadPerson.PlaceOfOrigin,
                x => x.SwissAbroadPerson.ReligionData,
                x => x.SwissAbroadPerson.Extension,
                (x, ext) => voter.SwissAbroadPerson = x.ToSwissAbroadPerson(ext!));
            return;
        }

        if (person?.Foreigner != null)
        {
            voter.VoterType = DataModels.VoterType.Foreigner;
            MapEchPersonToVoter(
                person.Foreigner,
                voter,
                x => x.ForeignerPerson.PersonIdentification,
                x => x.ForeignerPerson.LanguageOfCorrespondance,
                x => x.Municipality,
                _ => new List<PlaceOfOriginType>(),
                x => x.ForeignerPerson.ReligionData);
            return;
        }

        throw new InvalidOperationException("Person could not be mapped to voter.");
    }

    private static void MapEchPersonToVoter<TNationality>(
        TNationality nationality,
        DataModels.Voter voter,
        Func<TNationality, PersonIdentificationType> personIdentificationSelector,
        Func<TNationality, LanguageType> languageOfCorrespondanceSelector,
        Func<TNationality, SwissMunicipalityType?> municipalitySelector,
        Func<TNationality, List<PlaceOfOriginType>> placeOfOriginSelector,
        Func<TNationality, ReligionDataType?> religionSelector,
        Func<TNationality, object>? personExtensionSelector = null,
        Action<TNationality, SwissPersonExtension?>? extension = null)
    {
        voter.LanguageOfCorrespondence = LanguageMapping.ToLanguage(languageOfCorrespondanceSelector(nationality));
        voter.Religion = religionSelector(nationality)?.Religion;

        var personExtension = personExtensionSelector != null
            ? SwissPersonExtensionMapping.GetExtension(personExtensionSelector(nationality))
            : null;

        var personIdentification = personIdentificationSelector(nationality);
        voter.FirstName = personIdentification.FirstName;
        voter.LastName = personIdentification.OfficialName;
        voter.Sex = personIdentification.Sex.ToSexType();
        voter.PersonId = personIdentification.LocalPersonId.PersonId;
        voter.PersonIdCategory = personIdentification.LocalPersonId.PersonIdCategory;
        voter.DateOfBirth = personIdentification.DateOfBirth.ToDateString();

        voter.SendVotingCardsToDomainOfInfluenceReturnAddress = personExtension != null && personExtension.SendVotingCardsToDomainOfInfluenceReturnAddress.HasValue
            ? personExtension.SendVotingCardsToDomainOfInfluenceReturnAddress.Value
            : false;

        voter.PlacesOfOrigin = placeOfOriginSelector(nationality).ToList()
            .ConvertAll(x => new DataModels.VoterPlaceOfOrigin
            {
                Name = x.OriginName,
                Canton = x.Canton.ToCantonAbbreviation(),
            });

        var municipality = municipalitySelector(nationality);
        voter.Bfs = municipality?.MunicipalityId?.ToString() ?? UnknownBfs;
        voter.MunicipalityName = municipality?.MunicipalityName ?? UnknownMunicipalityName;

        extension?.Invoke(nationality, personExtension);
    }

    private static PersonMailAddressType GetEchElectoralAddress(DataModels.Voter voter)
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
