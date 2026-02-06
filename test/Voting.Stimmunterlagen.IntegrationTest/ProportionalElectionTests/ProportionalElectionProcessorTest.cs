// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using Abraxas.Voting.Basis.Shared.V1;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Common;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ProportionalElectionTests;

public class ProportionalElectionProcessorTest : BaseWriteableDbTest
{
    public ProportionalElectionProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ProportionalElectionCreated()
    {
        var id = Guid.Parse("b37464b2-dad5-48e8-981c-2fdef3710482");
        await TestEventPublisher.PublishTwice(new ProportionalElectionCreated
        {
            ProportionalElection = new ProportionalElectionEventData
            {
                Active = true,
                Id = id.ToString(),
                ContestId = ContestMockData.BundFutureId,
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                MandateAlgorithm = ProportionalElectionMandateAlgorithm.HagenbachBischoff,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.ZweckverbandGossauId,
            },
        });

        var election = await GetPoliticalBusiness(id);
        election.ShouldMatchChildSnapshot("election");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.PoliticalBusinessId == election.Id)
            .OrderBy(x => x.SecureConnectId)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.DomainOfInfluenceId)
            .Select(x => new { x.SecureConnectId, x.Role, x.DomainOfInfluence!.Name })
            .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
    }

    [Fact]
    public async Task ProportionalElectionUpdated()
    {
        await TestEventPublisher.PublishTwice(new ProportionalElectionUpdated
        {
            ProportionalElection = new ProportionalElectionEventData
            {
                Active = true,
                Id = ProportionalElectionMockData.BundFuture1Id,
                ContestId = ContestMockData.BundFutureId,
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                MandateAlgorithm = ProportionalElectionMandateAlgorithm.HagenbachBischoff,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
            },
        });

        var election = await GetPoliticalBusiness(ProportionalElectionMockData.BundFuture1Guid);
        election.ShouldMatchChildSnapshot("election");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.PoliticalBusinessId == election.Id)
            .OrderBy(x => x.SecureConnectId)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.DomainOfInfluenceId)
            .Select(x => new { x.SecureConnectId, x.Role, x.DomainOfInfluence!.Name })
            .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
    }

    [Fact]
    public async Task ProportionalElectionActiveStateUpdated()
    {
        await TestEventPublisher.PublishTwice(new ProportionalElectionActiveStateUpdated
        {
            ProportionalElectionId = ProportionalElectionMockData.BundFuture1Id,
            Active = false,
        });

        var pbBefore = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == ProportionalElectionMockData.BundFuture1Guid));
        pbBefore.Active
            .Should()
            .BeFalse();

        await TestEventPublisher.Publish(2, new ProportionalElectionActiveStateUpdated
        {
            ProportionalElectionId = ProportionalElectionMockData.BundFuture1Id,
            Active = true,
        });

        var pbAfter = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == ProportionalElectionMockData.BundFuture1Guid));
        pbAfter.Active
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task ProportionalElectionEVotingApprovalUpdated()
    {
        await TestEventPublisher.PublishTwice(new ProportionalElectionEVotingApprovalUpdated
        {
            ProportionalElectionId = ProportionalElectionMockData.BundFuture1Id,
            Approved = true,
        });

        var pbBefore = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == ProportionalElectionMockData.BundFuture1Guid));
        pbBefore.EVotingApproved
            .Should()
            .BeTrue();

        await TestEventPublisher.Publish(2, new ProportionalElectionEVotingApprovalUpdated
        {
            ProportionalElectionId = ProportionalElectionMockData.BundFuture1Id,
            Approved = false,
        });

        var pbAfter = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == ProportionalElectionMockData.BundFuture1Guid));
        pbAfter.EVotingApproved
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task ProportionalElectionDeleted()
    {
        var electionId = ProportionalElectionMockData.BundFuture1Guid;
        var attachmentId = Guid.Parse("7c5cd488-470a-4f89-8eb2-177d9344b499");
        var voterListId = Guid.Parse("c851a4aa-2b79-4573-a745-ed3963ef7f1c");

        await RunOnDb(async db =>
        {
            db.Attachments.Add(new()
            {
                Id = attachmentId,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
                PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
                {
                    new() { PoliticalBusinessId = electionId },
                },
            });

            db.VoterLists.Add(new()
            {
                Id = voterListId,
                ImportId = VoterListImportMockData.BundFutureApprovedStadtGossauGuid,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
                PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
                {
                    new() { PoliticalBusinessId = electionId },
                },
            });

            await db.SaveChangesAsync();
        });

        await TestEventPublisher.PublishTwice(new ProportionalElectionDeleted
        {
            ProportionalElectionId = electionId.ToString(),
        });

        (await RunOnDb(db => db.PoliticalBusinesses.AnyAsync(x => x.Id == electionId)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.PoliticalBusinessPermissions.AnyAsync(x => x.PoliticalBusinessId == electionId)))
            .Should()
            .BeFalse();

        // attachments should be deleted when it has no political business entries.
        (await RunOnDb(db => db.Attachments.AnyAsync(x => x.Id == attachmentId)))
            .Should()
            .BeFalse();

        // voterlists should be deleted when it has no political business entries.
        (await RunOnDb(db => db.VoterLists.AnyAsync(x => x.Id == voterListId)))
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task ProportionalElectionToNewContestMoved()
    {
        var newContestId = ContestMockData.BundFutureApprovedGuid;

        await TestEventPublisher.PublishTwice(new ProportionalElectionToNewContestMoved
        {
            ProportionalElectionId = ProportionalElectionMockData.BundFuture1Id,
            NewContestId = newContestId.ToString(),
        });

        var pb = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(pb => pb.Id == ProportionalElectionMockData.BundFuture1Guid));
        pb.ContestId.Should().Be(newContestId);
        pb.DomainOfInfluenceId.Should().Be(
            StimmunterlagenUuidV5.BuildContestDomainOfInfluence(newContestId, DomainOfInfluenceMockData.BundGuid));

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.PoliticalBusinessId == pb.Id)
            .OrderBy(x => x.SecureConnectId)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.DomainOfInfluenceId)
            .Select(x => new { x.SecureConnectId, x.Role, x.DomainOfInfluence!.ContestId, x.DomainOfInfluence!.Name })
            .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
        permissions.All(x => x.ContestId == newContestId).Should().BeTrue();
    }

    private async Task<Data.Models.PoliticalBusiness> GetPoliticalBusiness(Guid id)
    {
        var pb = await RunOnDb(
            db => db.PoliticalBusinesses
                .Include(x => x.Translations)
                .SingleAsync(x => x.Id == id),
            Languages.German);
        foreach (var translation in pb.Translations!)
        {
            translation.Id = Guid.Empty;
            translation.PoliticalBusiness = null;
        }

        return pb;
    }
}
