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

public class VoterAttachmentDictionaryTest
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
            BuildAttachment(BundId, 1, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 100) }, false, new[] { PbId1 }),
            BuildAttachment(BundId, 2, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, null) }, false, new[] { PbId1, PbId2 }),
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, true, new[] { PbId1 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, false, new[] { PbId2 }),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, false, new[] { PbId2 }),
        };
        var voters = new List<Voter>
        {
            BuildVoter(GossauId, true, new[] { PbId1 }),
            BuildVoter(GossauId, false, new[] { PbId1 }),
            BuildVoter(GossauId, true, new[] { PbId2 }),
        };

        var dict = new VoterAttachmentDictionary(attachments, false);

        var result = voters.ConvertAll(v => dict
            .GetAttachmentStations(v.List!.PoliticalBusinessEntries!.Select(e => e.PoliticalBusinessId).ToList(), v.IsHouseholder));

        result.Where(x => x == "15").Should().HaveCount(1);
        result.Where(x => x == "1").Should().HaveCount(1);
        result.Where(x => x == "B").Should().HaveCount(1);
    }

    [Fact]
    public void StationsByVoterListForPoliticalAssemblyShouldWork()
    {
        var attachments = new List<Attachment>
        {
            BuildAttachment(BundId, 1, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 100) }, true),
            BuildAttachment(BundId, 2, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, null) }, false),
            BuildAttachment(GossauId, 5, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, false),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, false),
            BuildAttachment(GossauId, 11, new[] { BuildDomainOfInfluenceAttachmentCount(GossauId, 50) }, false),
        };
        var voters = new List<Voter>
        {
            BuildVoter(GossauId, true),
            BuildVoter(GossauId, true),
            BuildVoter(GossauId, false),
        };

        var dict = new VoterAttachmentDictionary(attachments, true);
        var result = voters.ConvertAll(v => dict
            .GetAttachmentStations(v.List!.PoliticalBusinessEntries!.Select(e => e.PoliticalBusinessId).ToList(), v.IsHouseholder));

        result.Where(v => v == "5B").Should().HaveCount(1);
        result.Where(v => v == "15B").Should().HaveCount(2);
    }

    [Fact]
    public void EmptyStationsByVoterListShouldWork()
    {
        var voter = BuildVoter(GossauId, true, new[] { PbId1 });

        var dict = new VoterAttachmentDictionary(new List<Attachment>(), true);
        var result = dict
            .GetAttachmentStations(voter.List!.PoliticalBusinessEntries!.Select(e => e.PoliticalBusinessId).ToList(), voter.IsHouseholder);

        result.Should().Be(string.Empty);
    }

    private Voter BuildVoter(Guid doiId, bool isHouseholder, IEnumerable<Guid>? pbIds = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            IsHouseholder = isHouseholder,
            List = new()
            {
                DomainOfInfluenceId = doiId,
                PoliticalBusinessEntries = pbIds?.Select(pbId => new PoliticalBusinessVoterListEntry { PoliticalBusinessId = pbId }).ToList() ?? new(),
            },
        };

    private Attachment BuildAttachment(Guid doiId, int station, IEnumerable<DomainOfInfluenceAttachmentCount> doiAttachmentCounts, bool sendOnlyToHouseholder, IEnumerable<Guid>? pbIds = null)
    {
        var id = Guid.NewGuid();

        return new()
        {
            Id = id,
            DomainOfInfluenceId = doiId,
            DomainOfInfluenceAttachmentCounts = doiAttachmentCounts.ToList(),
            Station = station,
            SendOnlyToHouseholder = sendOnlyToHouseholder,
            PoliticalBusinessEntries = pbIds?.Select(pbId => new PoliticalBusinessAttachmentEntry { AttachmentId = id, PoliticalBusinessId = pbId }).ToList() ?? new(),
        };
    }

    private DomainOfInfluenceAttachmentCount BuildDomainOfInfluenceAttachmentCount(Guid doiId, int? requiredCount) =>
        new() { DomainOfInfluenceId = doiId, RequiredCount = requiredCount };
}
