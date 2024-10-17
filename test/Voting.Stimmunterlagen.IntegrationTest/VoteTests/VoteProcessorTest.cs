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

namespace Voting.Stimmunterlagen.IntegrationTest.VoteTests;

public class VoteProcessorTest : BaseWriteableDbTest
{
    public VoteProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task VoteCreated()
    {
        var id = Guid.Parse("6dba2f95-e925-4c69-8e82-1c823955d1a8");
        await TestEventPublisher.PublishTwice(new VoteCreated
        {
            Vote = new VoteEventData
            {
                Active = true,
                Id = id.ToString(),
                ContestId = ContestMockData.BundFutureId,
                InternalDescription = "internal 99",
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                ResultAlgorithm = VoteResultAlgorithm.PopularMajority,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
            },
        });

        var vote = await GetPoliticalBusiness(id);
        vote.ShouldMatchChildSnapshot("vote");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
                .Where(x => x.PoliticalBusinessId == vote.Id)
                .OrderBy(x => x.SecureConnectId)
                .ThenBy(x => x.Role)
                .Select(x => new { x.SecureConnectId, x.Role })
                .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
    }

    [Fact]
    public async Task VoteUpdated()
    {
        await TestEventPublisher.PublishTwice(new VoteUpdated
        {
            Vote = new VoteEventData
            {
                Active = true,
                Id = VoteMockData.BundFuture6Id,
                ContestId = ContestMockData.BundFutureId,
                InternalDescription = "internal 99",
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                ResultAlgorithm = VoteResultAlgorithm.PopularMajority,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.StadtUzwilId,
            },
        });

        var vote = await GetPoliticalBusiness(VoteMockData.BundFuture6Guid);
        vote.ShouldMatchChildSnapshot("vote");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
                .Where(x => x.PoliticalBusinessId == vote.Id)
                .OrderBy(x => x.SecureConnectId)
                .ThenBy(x => x.Role)
                .Select(x => new { x.SecureConnectId, x.Role })
                .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");

        var step = await RunOnDb(db => db.StepStates
            .SingleAsync(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid
                              && x.Step == Step.PoliticalBusinessesApproval));
        step.Approved.Should().BeFalse();
    }

    [Fact]
    public async Task VoteUpdatedShouldNotResetApprovedIfReplay()
    {
        await SetEventProcessingState(null, 2);
        await TestEventPublisher.Publish(new VoteUpdated
        {
            Vote = new VoteEventData
            {
                Active = true,
                Id = VoteMockData.BundFuture5Id,
                ContestId = ContestMockData.BundFutureId,
                InternalDescription = "internal 99",
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                ResultAlgorithm = VoteResultAlgorithm.PopularMajority,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.StadtUzwilId,
            },
        });

        var step = await RunOnDb(db => db.StepStates
            .SingleAsync(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid
                              && x.Step == Step.PoliticalBusinessesApproval));
        step.Approved.Should().BeTrue();

        var vote = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == VoteMockData.BundFuture6Guid));
        vote.Approved.Should().BeTrue();
    }

    [Fact]
    public async Task VoteActiveStateUpdated()
    {
        await TestEventPublisher.PublishTwice(new VoteActiveStateUpdated
        {
            Active = false,
            VoteId = VoteMockData.BundFuture1Id,
        });

        var pbBefore = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == VoteMockData.BundFuture1Guid));
        pbBefore.Active
            .Should()
            .BeFalse();

        await TestEventPublisher.Publish(2, new VoteActiveStateUpdated
        {
            Active = true,
            VoteId = VoteMockData.BundFuture1Id,
        });

        var pbAfter = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == VoteMockData.BundFuture1Guid));
        pbAfter.Active
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task VoteDeleted()
    {
        var voteId = VoteMockData.BundFuture1Guid;
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
                    new() { PoliticalBusinessId = voteId },
                },
            });

            db.VoterLists.Add(new()
            {
                Id = voterListId,
                ImportId = VoterListImportMockData.BundFutureApprovedStadtGossauGuid,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
                PoliticalBusinessEntries = new List<PoliticalBusinessVoterListEntry>
                {
                    new() { PoliticalBusinessId = voteId },
                },
            });

            await db.SaveChangesAsync();
        });

        await TestEventPublisher.PublishTwice(new VoteDeleted
        {
            VoteId = voteId.ToString(),
        });

        (await RunOnDb(db => db.PoliticalBusinesses.AnyAsync(x => x.Id == voteId)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.PoliticalBusinessPermissions.AnyAsync(x => x.PoliticalBusinessId == voteId)))
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
    public async Task VoteToNewContestMoved()
    {
        var newContestId = ContestMockData.BundFutureApprovedGuid;

        await TestEventPublisher.PublishTwice(new VoteToNewContestMoved
        {
            VoteId = VoteMockData.SchulgemeindeAndwilArneggFuture1Id,
            NewContestId = newContestId.ToString(),
        });

        var pb = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(pb => pb.Id == VoteMockData.SchulgemeindeAndwilArneggFuture1Guid));
        pb.ContestId.Should().Be(newContestId);
        pb.DomainOfInfluenceId.Should().Be(
            StimmunterlagenUuidV5.BuildContestDomainOfInfluence(newContestId, DomainOfInfluenceMockData.SchulgemeindeAndwilArneggGuid));

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.PoliticalBusinessId == pb.Id)
            .OrderBy(x => x.SecureConnectId)
            .ThenBy(x => x.Role)
            .Select(x => new { x.SecureConnectId, x.Role, x.DomainOfInfluence!.ContestId })
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
