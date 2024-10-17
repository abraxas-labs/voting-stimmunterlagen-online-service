// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Voting.Lib.Common;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardPrintFileTests;

public class VotingCardPrintFileBuilderTest : BaseWriteableDbTest
{
    private static readonly Guid PoliticalBusinessId1 = Guid.Parse("2d304f9e-12be-4e32-b214-48498aca99cc");
    private static readonly Guid PoliticalBusinessId2 = Guid.Parse("6827e4d7-9d62-4474-86e6-6abd6ca4d4b2");

    private readonly VotingCardPrintFileBuilder _votingCardPrintFileBuilder;

    public VotingCardPrintFileBuilderTest(TestApplicationFactory factory)
        : base(factory)
    {
        _votingCardPrintFileBuilder = GetService<VotingCardPrintFileBuilder>();
    }

    [Fact]
    public async Task ShouldWorkPrintA4AttachementA5()
    {
        var job = GetJob("voting_template");
        var attachments = GetAttachments(AttachmentFormat.A5);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkPrintA4AttachementA5)}.csv");
    }

    [Fact]
    public async Task ShouldWorkForPoliticalAssemblyPrintA4AttachementA5()
    {
        var job = GetJob("voting_template", true);
        var attachments = GetAttachments(AttachmentFormat.A5, true);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = true });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkForPoliticalAssemblyPrintA4AttachementA5)}.csv");
    }

    [Fact]
    public async Task ShouldWorkPrintA4AttachementA4()
    {
        var job = GetJob("voting_template");
        var attachments = GetAttachments(AttachmentFormat.A4);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkPrintA4AttachementA4)}.csv");
    }

    [Fact]
    public async Task ShouldWorkForPoliticalAssemblyPrintA4AttachementA4()
    {
        var job = GetJob("voting_template", true);
        var attachments = GetAttachments(AttachmentFormat.A4, true);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = true, });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkForPoliticalAssemblyPrintA4AttachementA4)}.csv");
    }

    [Fact]
    public async Task ShouldWorkPrintA5AttachementA5()
    {
        var job = GetJob("voting_template_a5");
        var attachments = GetAttachments(AttachmentFormat.A5);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkPrintA5AttachementA5)}.csv");
    }

    [Fact]
    public async Task ShouldWorkForPoliticalAssemblyPrintA5AttachementA5()
    {
        var job = GetJob("voting_template_a5", true);
        var attachments = GetAttachments(AttachmentFormat.A5, true);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = true });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkForPoliticalAssemblyPrintA5AttachementA5)}.csv");
    }

    [Fact]
    public async Task ShouldWorkWithEVoting()
    {
        var job = GetJob("voting_template_a5", false);

        // E-Voting has no templates
        job.Layout!.VotingCardType = VotingCardType.EVoting;
        job.Layout!.OverriddenTemplate = null!;
        var attachments = GetAttachments(AttachmentFormat.A5, false);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkWithEVoting)}.csv");
    }

    private VotingCardGeneratorJob GetJob(string templateName, bool isPoliticalAssembly = false)
    {
        var voterList1 = new VoterList()
        {
            Id = Guid.Parse("f1ac56e6-d9f1-4174-bf14-a7d23c390c93"),
            PoliticalBusinessEntries = isPoliticalAssembly
                ? new List<PoliticalBusinessVoterListEntry>()
                : new List<PoliticalBusinessVoterListEntry>()
                {
                    new() { PoliticalBusinessId = PoliticalBusinessId1 },
                },
        };
        var voterList2 = new VoterList()
        {
            Id = Guid.Parse("96c33f04-174a-45e9-a173-6783b3d4bf35"),
            PoliticalBusinessEntries = isPoliticalAssembly
                ? new List<PoliticalBusinessVoterListEntry>()
                : new List<PoliticalBusinessVoterListEntry>()
                {
                    new() { PoliticalBusinessId = PoliticalBusinessId1 },
                },
        };

        return new VotingCardGeneratorJob
        {
            DomainOfInfluence = new()
            {
                PrintData = new DomainOfInfluenceVotingCardPrintData
                {
                    ShippingAway = VotingCardShippingFranking.B1,
                    ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen,
                    ShippingReturn = VotingCardShippingFranking.WithoutFranking,
                },
                SwissPostData = new DomainOfInfluenceVotingCardSwissPostData
                {
                    InvoiceReferenceNumber = "666994573",
                    FrankingLicenceReturnNumber = "669302122",
                },
                ReturnAddress = new DomainOfInfluenceVotingCardReturnAddress
                {
                    City = "St. Gallen",
                    Country = "Switzerland",
                    Street = "Bahnhofplatz",
                    ZipCode = "9000",
                    AddressLine1 = "Rathaus",
                },
                Contest = new()
                {
                    OrderNumber = 999666,
                    IsPoliticalAssembly = isPoliticalAssembly,
                },
                VotingCardColor = VotingCardColor.Gold,
            },
            Voter = new List<Voter>
            {
                new Voter
                {
                    List = voterList1,
                    ListId = voterList1.Id,
                    FirstName = "Arnd",
                    LastName = "Thalberg",
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
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "Wil",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                    PageInfo = new()
                    {
                        PageFrom = 1,
                        PageTo = 2,
                    },
                },
                new Voter
                {
                    List = voterList2,
                    ListId = voterList2.Id,
                    FirstName = "Torsten",
                    LastName = "Meister",
                    Street = "Rhosddu Rd",
                    HouseNumber = "72",
                    Town = "Forest",
                    ForeignZipCode = "SA4 1RQ",
                    Country = { Iso2 = "GB", Name = "Vereinigtes Königreich" },
                    Bfs = "1234",
                    LanguageOfCorrespondence = Languages.Italian,
                    VotingCardType = VotingCardType.EVoting,
                    DateOfBirth = "1962-10-09",
                    Sex = SexType.Male,
                    VoterType = VoterType.SwissAbroad,
                    PersonId = "102",
                    PersonIdCategory = "Auslandschweizer",
                    SendVotingCardsToDomainOfInfluenceReturnAddress = true,
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
                    PlacesOfOrigin = new List<VoterPlaceOfOrigin>
                    {
                        new VoterPlaceOfOrigin
                        {
                            Name = "St. Gallen",
                            Canton = CantonAbbreviation.SG,
                        },
                    },
                    PageInfo = new()
                    {
                        PageFrom = 3,
                        PageTo = 4,
                    },
                },
            },
            Layout = new()
            {
                OverriddenTemplate = new() { InternName = templateName },
            },
        };
    }

    private List<Attachment> GetAttachments(AttachmentFormat format, bool isPoliticalAssembly = false)
    {
        return new()
        {
            new()
            {
                Station = 1,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 5 },
                },
                Format = format,
            },
            new()
            {
                Station = 1,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 5 },
                },
            },
            new()
            {
                Station = 12,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId2 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
            new()
            {
                Station = 11,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
            new()
            {
                Station = 7,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>(),
            },
            new()
            {
                Station = 6,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
        };
    }
}
