// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;
using PoliticalBusinessType = Voting.Stimmunterlagen.Data.Models.PoliticalBusinessType;

namespace Voting.Stimmunterlagen.IntegrationTest.MajorityElectionTests;

public class SecondaryMajorityElectionProcessorTest : BaseWriteableDbTest
{
    public SecondaryMajorityElectionProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SecondaryMajorityElectionCreated()
    {
        var id = Guid.Parse("1053ba81-811d-41b4-8625-5a6a17099e77");
        await TestEventPublisher.PublishTwice(new SecondaryMajorityElectionCreated
        {
            SecondaryMajorityElection = new SecondaryMajorityElectionEventData
            {
                Active = true,
                Id = id.ToString(),
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                NumberOfMandates = 10,
                PrimaryMajorityElectionId = MajorityElectionMockData.BundFuture1Id,
            },
        });

        var election = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == id));
        election.PoliticalBusinessType.Should().HaveSameValueAs(PoliticalBusinessType.SecondaryMajorityElection);

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
    public async Task SecondaryMajorityElectionUpdated()
    {
        await TestEventPublisher.PublishTwice(new SecondaryMajorityElectionUpdated
        {
            SecondaryMajorityElection = new SecondaryMajorityElectionEventData
            {
                Active = true,
                Id = SecondaryMajorityElectionMockData.BundFuture51Id,
                OfficialDescription = { LanguageUtil.MockAllLanguages("official 99") },
                ShortDescription = { LanguageUtil.MockAllLanguages("short 99") },
                PoliticalBusinessNumber = "99",
                NumberOfMandates = 10,
                PrimaryMajorityElectionId = MajorityElectionMockData.BundFuture5Id,
            },
        });

        var election = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == SecondaryMajorityElectionMockData.BundFuture51Guid));
        election.PoliticalBusinessType.Should().HaveSameValueAs(PoliticalBusinessType.SecondaryMajorityElection);

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
    public async Task SecondaryMajorityElectionActiveStateUpdated()
    {
        await TestEventPublisher.PublishTwice(new SecondaryMajorityElectionActiveStateUpdated
        {
            Active = false,
            SecondaryMajorityElectionId = SecondaryMajorityElectionMockData.BundFuture51Id,
        });

        var pbBefore = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == SecondaryMajorityElectionMockData.BundFuture51Guid));
        pbBefore.Active
            .Should()
            .BeFalse();

        await TestEventPublisher.Publish(2, new SecondaryMajorityElectionActiveStateUpdated
        {
            Active = true,
            SecondaryMajorityElectionId = SecondaryMajorityElectionMockData.BundFuture51Id,
        });

        var pbAfter = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == SecondaryMajorityElectionMockData.BundFuture51Guid));
        pbAfter.Active
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task SecondaryMajorityElectionEVotingApprovalUpdated()
    {
        await TestEventPublisher.PublishTwice(new SecondaryMajorityElectionEVotingApprovalUpdated
        {
            Approved = true,
            SecondaryMajorityElectionId = SecondaryMajorityElectionMockData.BundFuture51Id,
        });

        var pbBefore = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == SecondaryMajorityElectionMockData.BundFuture51Guid));
        pbBefore.EVotingApproved
            .Should()
            .BeTrue();

        await TestEventPublisher.Publish(2, new SecondaryMajorityElectionEVotingApprovalUpdated
        {
            Approved = false,
            SecondaryMajorityElectionId = SecondaryMajorityElectionMockData.BundFuture51Id,
        });

        var pbAfter = await RunOnDb(db => db.PoliticalBusinesses.SingleAsync(x => x.Id == SecondaryMajorityElectionMockData.BundFuture51Guid));
        pbAfter.EVotingApproved
            .Should()
            .BeFalse();
    }

    [Fact]
    public async Task SecondaryMajorityElectionDeleted()
    {
        var electionId = SecondaryMajorityElectionMockData.BundFuture51Guid;
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

        await TestEventPublisher.PublishTwice(new SecondaryMajorityElectionDeleted
        {
            SecondaryMajorityElectionId = electionId.ToString(),
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
}
