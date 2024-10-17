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

namespace Voting.Stimmunterlagen.IntegrationTest.MajorityElectionTests;

public class MajorityElectionProcessorTest : BaseWriteableDbTest
{
    public MajorityElectionProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task MajorityElectionCreated()
    {
        var id = Guid.Parse("4521fb7a-6ca8-484b-a42a-71068e8dc34e");
        await TestEventPublisher.PublishTwice(new MajorityElectionCreated
        {
            MajorityElection = new MajorityElectionEventData
            {
                Active = true,
                Id = id.ToString(),
                ContestId = ContestMockData.BundFutureId,
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                MandateAlgorithm = MajorityElectionMandateAlgorithm.AbsoluteMajority,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
            },
        });

        var election = await GetPoliticalBusiness(id);
        election.ShouldMatchChildSnapshot("election");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
                .Where(x => x.PoliticalBusinessId == id)
                .OrderBy(x => x.SecureConnectId)
                .ThenBy(x => x.Role)
                .Select(x => new { x.SecureConnectId, x.Role })
                .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
    }

    [Fact]
    public async Task MajorityElectionUpdated()
    {
        await TestEventPublisher.PublishTwice(new MajorityElectionUpdated
        {
            MajorityElection = new MajorityElectionEventData
            {
                Active = true,
                Id = MajorityElectionMockData.BundFuture1Id,
                ContestId = ContestMockData.BundFutureId,
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                MandateAlgorithm = MajorityElectionMandateAlgorithm.AbsoluteMajority,
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
            },
        });

        var election = await GetPoliticalBusiness(MajorityElectionMockData.BundFuture1Guid);
        election.ShouldMatchChildSnapshot("election");

        var permissions = await RunOnDb(db => db.PoliticalBusinessPermissions
                .Where(x => x.PoliticalBusinessId == election.Id)
                .OrderBy(x => x.SecureConnectId)
                .ThenBy(x => x.Role)
                .Select(x => new { x.SecureConnectId, x.Role })
                .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");
    }

    [Fact]
    public async Task MajorityElectionActiveStateUpdated()
    {
        await TestEventPublisher.PublishTwice(new MajorityElectionActiveStateUpdated
        {
            Active = false,
            MajorityElectionId = MajorityElectionMockData.BundFuture5Id,
        });

        var pbBefore = await GetPoliticalBusiness(MajorityElectionMockData.BundFuture5Guid);
        pbBefore.Active
            .Should()
            .BeFalse();

        await TestEventPublisher.Publish(2, new MajorityElectionActiveStateUpdated
        {
            Active = true,
            MajorityElectionId = MajorityElectionMockData.BundFuture5Id,
        });

        var pbAfter = await GetPoliticalBusiness(MajorityElectionMockData.BundFuture5Guid);
        pbAfter.Active
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task MajorityElectionDeleted()
    {
        var electionId = MajorityElectionMockData.BundFuture5Guid;
        var attachmentId = Guid.Parse("7c5cd488-470a-4f89-8eb2-177d9344b499");
        var voterListId = Guid.Parse("c851a4aa-2b79-4573-a745-ed3963ef7f1c");

        // necessary to test if print job state is changed to ReadyForProcess when it should be.
        await ModifyDbEntities<Attachment>(
            a => a.Id == AttachmentMockData.BundFutureApprovedGemeindeArneggGuid,
            a => a.State = AttachmentState.Delivered);

        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
            doi => doi.GenerateVotingCardsTriggered = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        await ModifyDbEntities<PrintJob>(
            p => p.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
            p => p.State = PrintJobState.SubmissionOngoing);

        await RunOnDb(async db =>
        {
            db.Attachments.RemoveRange(await db.Attachments
                .Where(a => a.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid
                    && a.Id != AttachmentMockData.BundFutureApprovedGemeindeArneggGuid).ToListAsync());

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

        await TestEventPublisher.PublishTwice(new MajorityElectionDeleted
        {
            MajorityElectionId = electionId.ToString(),
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

        // the print job state should be updated when necessary (in this case, when all attachments are delivered).
        (await RunOnDb(db => db.PrintJobs.SingleAsync(p => p.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid)))
            .State
            .Should()
            .Be(PrintJobState.ReadyForProcess);
    }

    [Fact]
    public async Task MajorityElectionToNewContestMoved()
    {
        var newContestId = ContestMockData.BundFutureApprovedGuid;

        await TestEventPublisher.PublishTwice(new MajorityElectionToNewContestMoved
        {
            MajorityElectionId = MajorityElectionMockData.BundFuture1Id,
            NewContestId = newContestId.ToString(),
        });

        var pb = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(pb => pb.Id == MajorityElectionMockData.BundFuture1Guid));
        pb.ContestId.Should().Be(newContestId);
        pb.DomainOfInfluenceId.Should().Be(
            StimmunterlagenUuidV5.BuildContestDomainOfInfluence(newContestId, DomainOfInfluenceMockData.BundGuid));

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
