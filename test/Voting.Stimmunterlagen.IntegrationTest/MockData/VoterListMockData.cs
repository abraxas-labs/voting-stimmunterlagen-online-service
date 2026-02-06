// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class VoterListMockData
{
    public const string BundArchivedGemeindeArneggSwissId = "71f06ac1-a944-4b37-94f0-c98a315369ff";
    public const string BundFutureApprovedGemeindeArneggSwissId = "00a09574-2e85-44c9-8da4-c4128cfc34a1";
    public const string BundFutureApprovedGemeindeArneggSwissElectoralRegisterId = "58176a58-d7d8-4936-825c-a93cb8f0c182";
    public const string BundFutureApprovedGemeindeArneggEVoterId = "b4c99c20-330e-4231-ae95-a5cbb05f1c20";
    public const string BundFutureApprovedStadtGossauSwissId = "f78400a8-b61d-4f7c-9445-53c2b70d56ee";
    public const string BundFutureApprovedStadtGossauEVoterId = "861529c0-4a6d-49c6-8fb6-fe1daf4106ee";
    public const string PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissId = "342af420-3422-4016-b545-3d7632230935";

    public static readonly Guid BundArchivedGemeindeArneggSwissGuid = Guid.Parse(BundArchivedGemeindeArneggSwissId);
    public static readonly Guid BundFutureApprovedGemeindeArneggSwissGuid = Guid.Parse(BundFutureApprovedGemeindeArneggSwissId);
    public static readonly Guid BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid = Guid.Parse(BundFutureApprovedGemeindeArneggSwissElectoralRegisterId);
    public static readonly Guid BundFutureApprovedGemeindeArneggEVoterGuid = Guid.Parse(BundFutureApprovedGemeindeArneggEVoterId);
    public static readonly Guid BundFutureApprovedStadtGossauSwissGuid = Guid.Parse(BundFutureApprovedStadtGossauSwissId);
    public static readonly Guid BundFutureApprovedStadtGossauEVoterGuid = Guid.Parse(BundFutureApprovedStadtGossauEVoterId);
    public static readonly Guid PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid = Guid.Parse(PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissId);

    public static VoterList BundArchivedGemeindeArneggSwiss => new()
    {
        Id = BundArchivedGemeindeArneggSwissGuid,
        ImportId = VoterListImportMockData.BundArchivedGemeindeArneggGuid,
        Index = 1,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
        VotingCardType = VotingCardType.Swiss,
        NumberOfVoters = 3,
        CountOfVotingCardsForHouseholders = 3,
        CountOfVotingCards = 3,
        SendVotingCardsToDomainOfInfluenceReturnAddress = true,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFuture1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFuture2Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFuture3Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureGemeindeArnegg1Guid },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("a3f28dd4-d61a-434a-9f30-d1e0f520631c"),
                    Salutation = Salutation.Mister,
                    FirstName = "Frank",
                    LastName = "Müller",
                    AddressLastName = "Müller",
                    Street = "Via Pestariso",
                    HouseNumber = "36",
                    Town = "St. Erhard",
                    SwissZipCode = 6212,
                    PostOfficeBoxText = "Postfach A",
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1991-03-10",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "1",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundArchivedGuid,
                    Religion = "111",
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "St. Gallen",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                    DomainOfInfluences = new List<VoterDomainOfInfluence>
                    {
                        new VoterDomainOfInfluence
                        {
                            DomainOfInfluenceIdentification = "AG-SC",
                            DomainOfInfluenceName = "Arnegg Schulkreis",
                            DomainOfInfluenceType = DomainOfInfluenceType.Sc,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("81b87c21-a25d-423d-9835-a1ea43975b9e"),
                    Salutation = Salutation.Mister,
                    FirstName = "Marco",
                    LastName = "Koch",
                    AddressLastName = "Koch",
                    Street = "Scheidweg",
                    HouseNumber = "12",
                    Town = "Gündlikon",
                    SwissZipCode = 8353,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1985-01-15",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "2",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundArchivedGuid,
                    Religion = "121",
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Thun",
                            Canton = CantonAbbreviation.BE,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("9b5efe6d-b4b4-4625-9272-cffca104648a"),
                    FirstName = "Arnd",
                    LastName = "Thalberg",
                    AddressLastName = "Thalberg",
                    Street = "Damunt",
                    HouseNumber = "149",
                    DwellingNumber = "2. Stock",
                    Town = "Gündlikon",
                    ForeignZipCode = "DE-91801",
                    Country = { Iso2 = "DE", Name = "Deutschland" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1980-07-21",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "3",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundArchivedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Winterthur",
                            Canton = CantonAbbreviation.ZH,
                        },
                    },
                },
            },
    };

    public static VoterList BundFutureApprovedGemeindeArneggSwiss => new()
    {
        Id = BundFutureApprovedGemeindeArneggSwissGuid,
        ImportId = VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid,
        Index = 1,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        VotingCardType = VotingCardType.Swiss,
        NumberOfVoters = 3,
        CountOfVotingCardsForHouseholders = 3,
        CountOfVotingCards = 3,
        CountOfVotingCardsForDomainOfInfluenceReturnAddress = 3,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedKantonStGallen1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedStadtUzwil1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedGemeindeArnegg1Guid },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("9b955013-a812-490c-8aaa-903a15082d4e"),
                    FirstName = "Frank",
                    LastName = "Müller",
                    AddressLastName = "Müller",
                    Street = "Via Pestariso",
                    HouseNumber = "36",
                    Town = "St. Erhard",
                    SwissZipCode = 6212,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1991-03-10",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "1",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "St. Gallen",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
                new Voter
                {
                    Id = Guid.Parse("b84cf6da-bb4a-48d0-b99c-41ca741adbf8"),
                    FirstName = "Marco",
                    LastName = "Koch",
                    AddressLastName = "Koch",
                    Street = "Scheidweg",
                    HouseNumber = "12",
                    Town = "Gündlikon",
                    SwissZipCode = 8353,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1985-01-15",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "2",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Thun",
                            Canton = CantonAbbreviation.BE,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
                new Voter
                {
                    Id = Guid.Parse("e13be747-b0b7-42f4-a5c1-4f589f7aed34"),
                    FirstName = "Arnd",
                    LastName = "Thalberg",
                    AddressLastName = "Thalberg",
                    Street = "Damunt",
                    HouseNumber = "149",
                    DwellingNumber = "2. Stock",
                    Town = "Gündlikon",
                    ForeignZipCode = "DE-91801",
                    Country = { Iso2 = "DE", Name = "Deutschland" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1980-07-21",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "3",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Winterthur",
                            Canton = CantonAbbreviation.ZH,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
            },
    };

    public static VoterList BundFutureApprovedGemeindeArneggSwissElectoralRegister => new()
    {
        Id = BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid,
        ImportId = VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterGuid,
        Index = 2,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        VotingCardType = VotingCardType.Swiss,
        NumberOfVoters = 2,
        CountOfVotingCardsForHouseholders = 2,
        CountOfVotingCards = 2,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedKantonStGallen1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedGemeindeArnegg1Guid },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("659a0ea5-08d4-4fb8-a0d8-3dc0c8ccc4b6"),
                    FirstName = "Marina",
                    LastName = "Kruger",
                    AddressLastName = "Kruger",
                    Street = "Breitenstrasse",
                    HouseNumber = "148",
                    Town = "Basel",
                    SwissZipCode = 4013,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "4433",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1986-03-12",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "2",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Basel",
                            Canton = CantonAbbreviation.BS,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("d447bd84-4f4e-4557-b41d-e9b986b607ef"),
                    FirstName = "Stephan",
                    LastName = "Ziegler",
                    AddressLastName = "Ziegler",
                    Street = "Via Gabbietta",
                    HouseNumber = "16",
                    DwellingNumber = "EG",
                    Town = "Neubrunn",
                    ForeignZipCode = "DE-91802",
                    Country = { Iso2 = "DE", Name = "Deutschland" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1974-06-12",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "97",
                    PersonIdCategory = "Auslandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Chur",
                            Canton = CantonAbbreviation.GR,
                        },
                    },
                },
            },
    };

    public static VoterList BundFutureApprovedGemeindeArneggEVoter => new()
    {
        Id = BundFutureApprovedGemeindeArneggEVoterGuid,
        ImportId = VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid,
        Index = 3,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        VotingCardType = VotingCardType.EVoting,
        NumberOfVoters = 4,
        CountOfVotingCardsForHouseholders = 4,
        CountOfVotingCards = 4,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedKantonStGallen1Guid },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("13d36239-2443-4b89-8df2-1593f1c55498"),
                    Salutation = Salutation.Miss,
                    FirstName = "Angelika",
                    LastName = "Herz",
                    AddressLastName = "Herz",
                    Street = "Adamsville Road",
                    HouseNumber = "535",
                    DwellingNumber = "13. Floor",
                    Town = "Laredo",
                    ForeignZipCode = "TX 78040",
                    Country = { Iso2 = "US", Name = "Vereinigte Staaten von Amerika" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    Religion = "111",
                    VotingCardType = VotingCardType.EVoting,
                    DateOfBirth = "1980-07-21",
                    Sex = SexType.Female,
                    VoterType = VoterType.Foreigner,
                    PersonId = "100",
                    PersonIdCategory = "Ausländer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                },
                new Voter
                {
                    Id = Guid.Parse("33e4f957-5280-4c38-a302-187dd2d85301"),
                    FirstName = "Torsten",
                    LastName = "Meister",
                    AddressLastName = "Meister",
                    Street = "Rhosddu Rd",
                    HouseNumber = "72",
                    Town = "Forest",
                    ForeignZipCode = "SA4 1RQ",
                    PostOfficeBoxText = "Postfach 27",
                    Country = { Iso2 = "GB", Name = "Vereinigtes Königreich" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.Italian,
                    VotingCardType = VotingCardType.EVoting,
                    DateOfBirth = "1962-10-09",
                    Sex = SexType.Male,
                    VoterType = VoterType.SwissAbroad,
                    PersonId = "102",
                    PersonIdCategory = "Auslandschweizer",
                    SwissAbroadPerson = new()
                    {
                        DateOfRegistration = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        ResidenceCountry = new() { Iso2 = "IT", Name = "Italien" },
                        Extension = new()
                        {
                            PostageCode = "2005",
                            Address = new()
                            {
                                Line1 = "Torsten Meister",
                                Line2 = "Rhosddu Rd 72",
                                Line3 = "Forest",
                                Line4 = "SA4 1RQ",
                                Line7 = "UNITED KINGDOM",
                            },
                            Authority = new()
                            {
                                Organisation = new()
                                {
                                    Name = "Staatskanzlei",
                                    AddOn1 = "St. Gallen",
                                },
                                AddressLine1 = "Kanton St. Gallen",
                                AddressLine2 = "Staatskanzlei",
                                Street = "Regierungsgebäude",
                                Town = "St. Gallen",
                                SwissZipCode = 9001,
                                Country = new()
                                {
                                    Iso2 = "CH",
                                    Name = "SWITZERLAND",
                                },
                            },
                        },
                    },
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Zürich",
                            Canton = CantonAbbreviation.ZH,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("0cf6fa4c-974c-4ac5-aedb-d68d4733c54d"),
                    FirstName = "Patrik",
                    LastName = "Kronig",
                    AddressLastName = "Kronig",
                    Street = "Rissik St",
                    HouseNumber = "1930",
                    Town = "Groblersdal",
                    ForeignZipCode = "1061",
                    Country = { Iso2 = "SA", Name = "Südafrika" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.Italian,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1991-02",
                    Sex = SexType.Undefined,
                    VoterType = VoterType.SwissAbroad,
                    PersonId = "103",
                    PersonIdCategory = "Auslandschweizer",
                    SwissAbroadPerson = new()
                    {
                        DateOfRegistration = new DateTime(2005, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                        ResidenceCountry = new()
                        {
                            Iso2 = "GB",
                            Name = "Vereinigtes Königreich",
                        },
                        Extension = new()
                        {
                            PostageCode = "2005",
                            Address = new()
                            {
                                Line1 = "Patrick Kronig",
                                Line2 = "Rissik St",
                                Line3 = "Groblersdal",
                                Line4 = "1061",
                                Line7 = "SOUTH AFRICA",
                            },
                            Authority = new()
                            {
                                Organisation = new()
                                {
                                    Name = "Staatskanzlei",
                                    AddOn1 = "St. Gallen",
                                },
                                AddressLine1 = "Kanton St. Gallen",
                                AddressLine2 = "Staatskanzlei",
                                Street = "Regierungsgebäude",
                                Town = "St. Gallen",
                                SwissZipCode = 9001,
                                Country = new()
                                {
                                    Iso2 = "CH",
                                    Name = "SWITZERLAND",
                                },
                            },
                        },
                    },
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Basel",
                            Canton = CantonAbbreviation.BS,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("ba9f5a53-970c-40ab-a217-25cee80ed390"),
                    FirstName = "Matthias",
                    LastName = "Baumgartner",
                    AddressFirstName = "Matthias",
                    AddressLastName = "Baumgartner",
                    Street = "Diesel Street",
                    HouseNumber = "1528",
                    Town = "Bekkersdal",
                    ForeignZipCode = "1782",
                    Country = { Iso2 = "SA", Name = "Südafrika" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.EVoting,
                    DateOfBirth = "1965",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "104",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Gossau",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                },
            },
    };

    public static VoterList BundFutureApprovedStadtGossauSwiss => new()
    {
        Id = BundFutureApprovedStadtGossauSwissGuid,
        ImportId = VoterListImportMockData.BundFutureApprovedStadtGossauGuid,
        Index = 1,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
        VotingCardType = VotingCardType.Swiss,
        NumberOfVoters = 3,
        CountOfVotingCardsForHouseholders = 3,
        CountOfVotingCards = 3,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedKantonStGallen1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedStadtUzwil1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedStadtGossau1Guid, },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = MajorityElectionMockData.BundFutureApprovedKantonStGallen1Guid, },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("c29258df-e96c-4a27-b058-bf2ca9f190db"),
                    FirstName = "Steffen",
                    LastName = "Walter",
                    AddressLastName = "Walter",
                    Street = "Fortunastrasse",
                    HouseNumber = "52",
                    Town = "Erlosen",
                    SwissZipCode = 8340,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "5555",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1980-07-21",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "200",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Gossau",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Bellinzona",
                            Canton = CantonAbbreviation.TI,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("80a38a13-1d93-46b8-86c2-cc188726a154"),
                    FirstName = "Brigitte",
                    LastName = "Hoffmann",
                    AddressLastName = "Hoffmann",
                    Street = "Forrenböhlstrasse",
                    HouseNumber = "16",
                    Town = "Kappel am Albis",
                    SwissZipCode = 8926,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "5555",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1984-03-12",
                    Sex = SexType.Female,
                    VoterType = VoterType.Swiss,
                    PersonId = "201",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Gossau",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Aarau",
                            Canton = CantonAbbreviation.AG,
                        },
                    },
                },
                new Voter
                {
                    Id = Guid.Parse("c5150c3f-6c91-4b0e-a849-55ec293ff7bf"),
                    FirstName = "Rebecca",
                    LastName = "Franziska",
                    AddressLastName = "Franziska",
                    Street = "Feierabend",
                    HouseNumber = "66",
                    DwellingNumber = "5. Stock",
                    Town = "Gündlikon",
                    SwissZipCode = 1583,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "5555",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1970-01",
                    Sex = SexType.Female,
                    VoterType = VoterType.Swiss,
                    PersonId = "202",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Gossau",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Luzern",
                            Canton = CantonAbbreviation.LU,
                        },
                    },
                },
            },
    };

    public static VoterList BundFutureApprovedStadtGossauEVoter => new()
    {
        Id = BundFutureApprovedStadtGossauEVoterGuid,
        ImportId = VoterListImportMockData.BundFutureApprovedStadtGossauGuid,
        Index = 1,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
        VotingCardType = VotingCardType.EVoting,
        NumberOfVoters = 1,
        CountOfVotingCardsForHouseholders = 1,
        CountOfVotingCards = 1,
        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
            {
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new PoliticalBusinessVoterListEntry { PoliticalBusinessId = VoteMockData.BundFutureApprovedStadtGossau1Guid, },
            },
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("f5b47aa3-414a-421a-aab0-d668f4fc9a9d"),
                    FirstName = "Steffen",
                    LastName = "Walter",
                    AddressLastName = "Walter",
                    Street = "Fortunastrasse",
                    HouseNumber = "52",
                    Town = "Erlosen",
                    SwissZipCode = 8340,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "5555",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1952",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "300",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Gossau",
                    ContestId = ContestMockData.BundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Bellinzona",
                            Canton = CantonAbbreviation.TI,
                        },
                    },
                },
            },
    };

    public static VoterList PoliticalAssemblyBundFutureApprovedGemeindeArneggSwiss => new()
    {
        Id = PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid,
        ImportId = VoterListImportMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
        Index = 1,
        DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
        VotingCardType = VotingCardType.Swiss,
        NumberOfVoters = 3,
        CountOfVotingCardsForHouseholders = 3,
        CountOfVotingCards = 3,
        CountOfVotingCardsForDomainOfInfluenceReturnAddress = 3,
        Voters = new List<Voter>
            {
                new Voter
                {
                    Id = Guid.Parse("8609a604-306c-4981-ae5a-51163fcd7b61"),
                    FirstName = "Frank",
                    LastName = "Müller",
                    AddressLastName = "Müller",
                    Street = "Via Pestariso",
                    HouseNumber = "36",
                    Town = "St. Erhard",
                    SwissZipCode = 6212,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1991-03-10",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "1",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "St. Gallen",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
                new Voter
                {
                    Id = Guid.Parse("85ea06be-9f53-410a-b1fb-1075aba97b73"),
                    FirstName = "Marco",
                    LastName = "Koch",
                    AddressLastName = "Koch",
                    Street = "Scheidweg",
                    HouseNumber = "12",
                    Town = "Gündlikon",
                    SwissZipCode = 8353,
                    Country = { Iso2 = "CH", Name = "Schweiz" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1985-01-15",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "2",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Thun",
                            Canton = CantonAbbreviation.BE,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
                new Voter
                {
                    Id = Guid.Parse("d7a9d89c-0295-4f54-9e59-5a16d2b5bb45"),
                    FirstName = "Arnd",
                    LastName = "Thalberg",
                    AddressLastName = "Thalberg",
                    Street = "Damunt",
                    HouseNumber = "149",
                    DwellingNumber = "2. Stock",
                    Town = "Gündlikon",
                    ForeignZipCode = "DE-91801",
                    Country = { Iso2 = "DE", Name = "Deutschland" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.German,
                    VotingCardType = VotingCardType.Swiss,
                    DateOfBirth = "1980-07-21",
                    Sex = SexType.Male,
                    VoterType = VoterType.Swiss,
                    PersonId = "3",
                    PersonIdCategory = "Inlandschweizer",
                    MunicipalityName = "Arnegg",
                    ContestId = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid,
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Winterthur",
                            Canton = CantonAbbreviation.ZH,
                        },
                    },
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                },
            },
    };

    public static IEnumerable<VoterList> All
    {
        get
        {
            yield return BundArchivedGemeindeArneggSwiss;
            yield return BundFutureApprovedGemeindeArneggSwiss;
            yield return BundFutureApprovedGemeindeArneggSwissElectoralRegister;
            yield return BundFutureApprovedGemeindeArneggEVoter;
            yield return BundFutureApprovedStadtGossauSwiss;
            yield return BundFutureApprovedStadtGossauEVoter;
            yield return PoliticalAssemblyBundFutureApprovedGemeindeArneggSwiss;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        var all = All.ToList();
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.VoterLists.AddRange(all);
            await db.SaveChangesAsync();

            await sp.GetRequiredService<DomainOfInfluenceAttachmentCountRepo>().UpdateRequiredForVoterListsCount();

            var attachmentIds = await db.Attachments.Select(x => x.Id).ToListAsync();
            foreach (var attachmentId in attachmentIds)
            {
                await sp.GetRequiredService<AttachmentRepo>().UpdateTotalCounts(attachmentId);
            }
        });
    }
}
