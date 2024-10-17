// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class AttachmentStationsBuilderTest
{
    private static readonly Guid BundId = Guid.Parse("8adb8a7e-d1b5-4ec6-83e2-8098bfe8f57b");
    private static readonly Guid GossauId = Guid.Parse("d8cfa74a-0890-4f25-971a-aa87628e7ede");
    private static readonly Guid ArneggId = Guid.Parse("6adc5c4b-8825-409e-97f1-a7e8bd61c121");

    private static readonly Guid PbId1 = Guid.Parse("e68d340a-d8ab-4983-a796-707f9ee0f0b8");
    private static readonly Guid PbId2 = Guid.Parse("6f89f347-a382-4d82-a3d8-96a2f6fcfc6f");

    [Fact]
    public void StationsByVoterListShouldWork()
    {
        var attachments = new List<Attachment>
        {
            BuildAttachment(BundId, 1, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 100) }, new[] { PbId1 }),
            BuildAttachment(BundId, 2, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, null) }, new[] { PbId1, PbId2 }),
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId1 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId2 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId2 }),
        };
        var voterLists = new List<VoterList>
        {
            BuildVoterList(GossauId, new[] { PbId1 }),
            BuildVoterList(GossauId, new[] { PbId1 }),
            BuildVoterList(GossauId, new[] { PbId2 }),
        };

        var result = AttachmentStationsBuilder.BuildAttachmentStationsByVoterListId(voterLists, attachments, false);
        result.Count.Should().Be(3);
        result.Values.Where(v => v == "15").Should().HaveCount(2);
        result.ContainsValue("B").Should().BeTrue();
    }

    [Fact]
    public void StationsByVoterListForPoliticalAssemblyShouldWork()
    {
        var attachments = new List<Attachment>
        {
            BuildAttachment(BundId, 1, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 100) }),
            BuildAttachment(BundId, 2, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, null) }),
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
        };
        var voterLists = new List<VoterList>
        {
            BuildVoterList(GossauId),
            BuildVoterList(GossauId),
            BuildVoterList(GossauId),
        };

        var result = AttachmentStationsBuilder.BuildAttachmentStationsByVoterListId(voterLists, attachments, true);
        result.Count.Should().Be(3);
        result.Values.Where(v => v == "15B").Should().HaveCount(3);
    }

    [Fact]
    public void StationsByDomainOfInfluenceShouldWork()
    {
        var attachments = new List<Attachment>
        {
            BuildAttachment(BundId, 1, new[] { BuildDomainOfInfluenceAttachmentCount(BundId, 0), BuildDomainOfInfluenceAttachmentCount(GossauId, 100) }, new[] { PbId1 }),
            BuildAttachment(BundId, 2, new[] { BuildDomainOfInfluenceAttachmentCount(BundId, 0), BuildDomainOfInfluenceAttachmentCount(GossauId, null), BuildDomainOfInfluenceAttachmentCount(ArneggId, null) }, new[] { PbId1, PbId2 }),
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId1 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId2 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, new[] { PbId2 }),
            BuildAttachment(ArneggId, 8, new[] { BuildDomainOfInfluenceAttachmentCount(ArneggId, 50) }, new[] { PbId2 }),
        };
        var voterLists = new List<VoterList>
        {
            BuildVoterList(GossauId, new[] { PbId1 }),
            BuildVoterList(GossauId, new[] { PbId2 }),
            BuildVoterList(ArneggId, new[] { PbId2 }),
        };

        var result = AttachmentStationsBuilder.BuildAttachmentStationsByDomainOfInfluenceId(voterLists, attachments, false);
        result.Count.Should().Be(2);
        result[GossauId].Should().Be("15B");
        result[ArneggId].Should().Be("8");
    }

    [Fact]
    public void StationsByDomainOfInfluenceForPoliticalAssemblyShouldWork()
    {
        var attachments = new List<Attachment>
        {
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }),
        };
        var voterLists = new List<VoterList>
        {
            BuildVoterList(GossauId),
            BuildVoterList(GossauId),
        };

        var result = AttachmentStationsBuilder.BuildAttachmentStationsByDomainOfInfluenceId(voterLists, attachments, true);
        result.Count.Should().Be(1);
        result[GossauId].Should().Be("5B");
    }

    [Fact]
    public void EmptyStationsByVoterListShouldWork()
    {
        var result = AttachmentStationsBuilder.BuildAttachmentStationsByVoterListId(new[] { BuildVoterList(GossauId, new[] { PbId1 }) }, new List<Attachment>(), false);
        result.Count.Should().Be(1);
        result.ContainsValue(string.Empty).Should().BeTrue();
    }

    [Fact]
    public void EmptyStationsByDomainOfInfluenceShouldWork()
    {
        var result = AttachmentStationsBuilder.BuildAttachmentStationsByDomainOfInfluenceId(new[] { BuildVoterList(GossauId, new[] { PbId1 }) }, new List<Attachment>(), false);
        result.Count.Should().Be(1);
        result.ContainsValue(string.Empty).Should().BeTrue();
    }

    private VoterList BuildVoterList(Guid doiId, IEnumerable<Guid>? pbIds = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            DomainOfInfluenceId = doiId,
            PoliticalBusinessEntries = pbIds?.Select(pbId => new PoliticalBusinessVoterListEntry { PoliticalBusinessId = pbId }).ToList() ?? new(),
        };

    private Attachment BuildAttachment(Guid doiId, int station, IEnumerable<DomainOfInfluenceAttachmentCount> doiAttachmentCounts, IEnumerable<Guid>? pbIds = null) =>
        new()
        {
            DomainOfInfluenceId = doiId,
            DomainOfInfluenceAttachmentCounts = doiAttachmentCounts.ToList(),
            Station = station,
            PoliticalBusinessEntries = pbIds?.Select(pbId => new PoliticalBusinessAttachmentEntry { PoliticalBusinessId = pbId }).ToList() ?? new(),
        };

    private DomainOfInfluenceAttachmentCount BuildDomainOfInfluenceAttachmentCount(Guid doiId, int? requiredCount) =>
        new() { DomainOfInfluenceId = doiId, RequiredCount = requiredCount };
}
