// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private static readonly Guid VoterListId1 = Guid.Parse("f1ac56e6-d9f1-4174-bf14-a7d23c390c93");
    private static readonly Guid VoterListId2 = Guid.Parse("96c33f04-174a-45e9-a173-6783b3d4bf35");

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
    public async Task ShouldWorkPrintA5AttachementA5Duplex()
    {
        var job = GetJob("voting_template_a5_duplex");
        var attachments = GetAttachments(AttachmentFormat.A5);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkPrintA5AttachementA5Duplex)}.csv");
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

        // E-Voting has no Layout
        job.Layout = null;
        job.State = VotingCardGeneratorJobState.ReadyToRunOffline;
        var attachments = GetAttachments(AttachmentFormat.A5, false);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkWithEVoting)}.csv");
    }

    [Fact]
    public async Task ShouldWorkWithDuplicates()
    {
        var job = GetJob("voting_template");

        var voter = job.Voter.First();
        voter.VoterDuplicateId = Guid.Empty;
        voter.VoterDuplicate = new DomainOfInfluenceVoterDuplicate
        {
            Voters = new List<Voter>
            {
                voter,
                new Voter { ListId = VoterListId2 },
            },
        };

        var attachments = GetAttachments(AttachmentFormat.A4);
        var entries = _votingCardPrintFileBuilder.MapToPrintFileEntries(job, attachments, new() { OrderNumber = 955000, IsPoliticalAssembly = false });
        entries.MatchSnapshot("rawEntries");

        var csvBytes = await _votingCardPrintFileBuilder.BuildPrintFile(job, attachments);
        using var ms = new MemoryStream(csvBytes);
        using var streamReader = new StreamReader(ms);
        var csv = streamReader.ReadToEnd();
        csv.MatchRawSnapshot("VotingCardPrintFileTests", "_snapshots", $"{nameof(VotingCardPrintFileBuilderTest)}_{nameof(ShouldWorkWithDuplicates)}.csv");
    }

    private VotingCardGeneratorJob GetJob(string templateName, bool isPoliticalAssembly = false)
    {
        var voterList1 = new VoterList()
        {
            Id = VoterListId1,
            PoliticalBusinessEntries = isPoliticalAssembly
                ? new List<PoliticalBusinessVoterListEntry>()
                : new List<PoliticalBusinessVoterListEntry>()
                {
                    new() { PoliticalBusinessId = PoliticalBusinessId1 },
                },
        };
        var voterList2 = new VoterList()
        {
            Id = VoterListId2,
            PoliticalBusinessEntries = isPoliticalAssembly
                ? new List<PoliticalBusinessVoterListEntry>()
                : new List<PoliticalBusinessVoterListEntry>()
                {
                    new() { PoliticalBusinessId = PoliticalBusinessId2 },
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
                    FrankingLicenceAwayNumber = "73011100",
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
                VoterLists = new List<VoterList>
                {
                    voterList1,
                    voterList2,
                },
                VotingCardColor = VotingCardColor.Green,
            },
            Voter = new List<Voter>
            {
                new Voter
                {
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
                    IsHouseholder = true,
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
        var id1 = Guid.Parse("9206346c-ab33-477a-a0d3-4b10347ad8a8");
        var id2 = Guid.Parse("07af1ef1-40c7-4fbe-af64-022b045dfcad");
        var id3 = Guid.Parse("f7759e92-1c3b-4d15-8a90-4567e049c207");
        var id4 = Guid.Parse("b49b3fd1-20b2-4d49-88fa-679a96bc8fe6");
        var id5 = Guid.Parse("41a1a84b-e308-45e3-9ee5-26dbced6e2e6");
        var id6 = Guid.Parse("3d630880-759f-4764-9060-aff6ea3a2bcf");
        var id7 = Guid.Parse("12b4fd54-dd95-4a8c-afdd-6b379e19ccba");

        return new()
        {
            new()
            {
                Id = id1,
                Station = 1,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1, AttachmentId = id1 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 5 },
                },
                Format = format,
            },
            new()
            {
                Id = id2,
                Station = 1,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1, AttachmentId = id2 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 5 },
                },
            },
            new()
            {
                Id = id3,
                Station = 12,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId2, AttachmentId = id3 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
                SendOnlyToHouseholder = true,
            },
            new()
            {
                Id = id4,
                Station = 11,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1, AttachmentId = id4 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
            new()
            {
                Id = id5,
                Station = 7,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1, AttachmentId = id5 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>(),
            },
            new()
            {
                Id = id6,
                Station = 6,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId1, AttachmentId = id6 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
            new()
            {
                Id = id7,
                Station = 9,
                PoliticalBusinessEntries = isPoliticalAssembly
                    ? new List<PoliticalBusinessAttachmentEntry>()
                    : new List<PoliticalBusinessAttachmentEntry>()
                    {
                        new() { PoliticalBusinessId = PoliticalBusinessId2, AttachmentId = id7 },
                    },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { RequiredCount = 1 },
                },
            },
        };
    }
}
