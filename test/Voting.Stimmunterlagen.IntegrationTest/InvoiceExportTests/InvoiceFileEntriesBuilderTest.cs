// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Core.Managers.Invoice;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.InvoiceExportTests;

public class InvoiceFileEntriesBuilderTest : BaseWriteableDbTest
{
    private static readonly Guid PoliticalBusinessId1 = Guid.Parse("48f37fed-34ae-48d3-ac0e-55d81c35bbc9");
    private static readonly Guid PoliticalBusinessId2 = Guid.Parse("d0837d3c-f450-4eb7-88ee-89577fbf5a03");

    private readonly InvoiceFileEntriesBuilder _builder;

    public InvoiceFileEntriesBuilderTest(TestApplicationFactory factory)
        : base(factory)
    {
        _builder = GetService<InvoiceFileEntriesBuilder>();
    }

    [Fact]
    public void ShouldWork()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithPoliticalAssembly()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(),
            MockedClock.GetDate(),
            BuildContest(true));
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithAttachmentA4()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.DomainOfInfluenceAttachmentCounts!.First().Attachment!.Format = AttachmentFormat.A4),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithVotingCardA5()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.VotingCardLayouts!.First().OverriddenTemplate!.InternName = "test_template_a5"),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithVotingCardA4AndDuplex()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.VotingCardLayouts!.First().OverriddenTemplate!.InternName = "test_template_duplex"),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithVotingCardA5AndDuplex()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.VotingCardLayouts!.First().OverriddenTemplate!.InternName = "test_template_duplex_a5"),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void ShouldWorkWithExcludedFlatrate()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.VotingCardFlatRateDisabled = true),
            MockedClock.GetDate(),
            BuildContest());
        entries.ShouldMatchSnapshot();
    }

    [Fact]
    public void TestBallotEnvelopeStandardExclCustom()
    {
        var ballotEnvelopeStandardExclCustomMaterialNumber = "1040.05.49";

        var printJob = BuildPrintJob();
        var doiAttachmentCounts = printJob.DomainOfInfluence!.DomainOfInfluenceAttachmentCounts!.ToList();
        doiAttachmentCounts[0].Attachment!.Category = AttachmentCategory.BallotEnvelopeStandard;
        doiAttachmentCounts[1].Attachment!.Category = AttachmentCategory.BallotEnvelopeCustom;

        // Should only have the material if it has ballot envelope standard > 0 with no ballot envelope custom.
        doiAttachmentCounts[0].RequiredCount = 10;
        doiAttachmentCounts[1].RequiredCount = 0;

        _builder.BuildEntries(printJob, MockedClock.GetDate(), BuildContest())
            .Any(e => e.SapMaterialNumber == ballotEnvelopeStandardExclCustomMaterialNumber && e.Amount == 60)
            .Should().BeTrue();

        _builder.BuildEntries(printJob, MockedClock.GetDate(), BuildContest(true))
            .Any(e => e.SapMaterialNumber == ballotEnvelopeStandardExclCustomMaterialNumber)
            .Should().BeFalse();

        doiAttachmentCounts[0].RequiredCount = 0;
        doiAttachmentCounts[1].RequiredCount = 0;
        _builder.BuildEntries(printJob, MockedClock.GetDate(), BuildContest())
            .Any(e => e.SapMaterialNumber == ballotEnvelopeStandardExclCustomMaterialNumber)
            .Should().BeFalse();

        doiAttachmentCounts[0].RequiredCount = 10;
        doiAttachmentCounts[1].RequiredCount = 10;
        _builder.BuildEntries(printJob, MockedClock.GetDate(), BuildContest())
            .Any(e => e.SapMaterialNumber == ballotEnvelopeStandardExclCustomMaterialNumber)
            .Should().BeFalse();

        doiAttachmentCounts[0].RequiredCount = 0;
        doiAttachmentCounts[1].RequiredCount = 10;
        _builder.BuildEntries(printJob, MockedClock.GetDate(), BuildContest())
            .Any(e => e.SapMaterialNumber == ballotEnvelopeStandardExclCustomMaterialNumber)
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldWorkEmpty()
    {
        var entries = _builder.BuildEntries(
            BuildPrintJob(p => p.DomainOfInfluence!.VoterLists = new List<VoterList>()),
            MockedClock.GetDate(),
            BuildContest());
        entries.Should().HaveCount(0);
    }

    private PrintJob BuildPrintJob(Action<PrintJob>? action = null)
    {
        var printJob = new PrintJob()
        {
            DomainOfInfluence = new()
            {
                SapCustomerOrderNumber = "00987",
                VotingCardLayouts = new List<DomainOfInfluenceVotingCardLayout>
                {
                    new()
                    {
                        VotingCardType = VotingCardType.Swiss,
                        OverriddenTemplate = new()
                        {
                            InternName = "test_template",
                        },
                    },
                },
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
                {
                    new()
                    {
                        RequiredCount = 60,
                        RequiredForVoterListsCount = 50,
                        Attachment = new()
                        {
                            Category = AttachmentCategory.BrochureCh,
                            Station = 1,
                            PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
                            {
                                new() { PoliticalBusinessId = PoliticalBusinessId1 },
                            },
                        },
                    },
                    new()
                    {
                        RequiredCount = 10,
                        RequiredForVoterListsCount = 10,
                        Attachment = new()
                        {
                            Category = AttachmentCategory.BrochureMu,
                            Station = 2,
                            PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
                            {
                                new() { PoliticalBusinessId = PoliticalBusinessId2 },
                            },
                        },
                    },
                },
                VoterLists = new List<VoterList>
                {
                    new()
                    {
                        NumberOfVoters = 50,
                        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
                        {
                            new() { PoliticalBusinessId = PoliticalBusinessId1 },
                        },
                    },
                    new()
                    {
                        NumberOfVoters = 10,
                        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
                        {
                            new() { PoliticalBusinessId = PoliticalBusinessId2 },
                        },
                    },
                    new()
                    {
                        NumberOfVoters = 2,
                        CountOfSendVotingCardsToDomainOfInfluenceReturnAddress = 2,
                        SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                        PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
                        {
                            new() { PoliticalBusinessId = PoliticalBusinessId2 },
                        },
                    },
                },
                AdditionalInvoicePositions = new List<AdditionalInvoicePosition>
                {
                    new()
                    {
                        MaterialNumber = "1040.05.57",
                        AmountCentime = 125,
                        Comment = "Testcomment",
                    },
                    new()
                    {
                        MaterialNumber = "Zusatzleistung",
                        AmountCentime = 975,
                    },
                },
            },
        };

        action?.Invoke(printJob);
        return printJob;
    }

    private Contest BuildContest(bool isPoliticalAssembly = false)
    {
        return new Contest
        {
            IsPoliticalAssembly = isPoliticalAssembly,
        };
    }
}
