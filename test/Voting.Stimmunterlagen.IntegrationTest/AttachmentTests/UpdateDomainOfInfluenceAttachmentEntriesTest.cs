// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class UpdateDomainOfInfluenceAttachmentEntriesTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public UpdateDomainOfInfluenceAttachmentEntriesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        var request = NewValidRequest();
        await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(request);

        var attachment = await RunOnDb(db => db.Attachments.Include(x => x.DomainOfInfluenceAttachmentCounts).SingleAsync(x => x.Id == Guid.Parse(request.Id)));
        attachment.TotalRequiredCount.Should().Be(40);
        attachment.DomainOfInfluenceAttachmentCounts!.Count.Should().Be(3); // owner and request doi entries.

        var request2 = NewValidRequest(x =>
        {
            x.DomainOfInfluenceIds.Clear();
            x.DomainOfInfluenceIds.Add(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId);
        });
        await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(request2);

        attachment = await RunOnDb(db => db.Attachments.Include(x => x.DomainOfInfluenceAttachmentCounts).SingleAsync(x => x.Id == Guid.Parse(request.Id)));
        attachment.TotalRequiredCount.Should().Be(0); // if a doi attachment count is removed, it should also be reflected in the total count.
        attachment.DomainOfInfluenceAttachmentCounts!.Count.Should().Be(2); // owner and request doi entries.
    }

    [Fact]
    public async Task ShouldUpdateWithExternalPrintingCenterEntry()
    {
        await RunOnDb(async db =>
        {
            db.DomainOfInfluenceAttachmentCounts.RemoveRange(await db.DomainOfInfluenceAttachmentCounts.ToListAsync());
            await db.SaveChangesAsync();
        });

        var externalPrintingCenterDoiId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid;

        await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(
            NewValidRequest(x => x.DomainOfInfluenceIds.Add(externalPrintingCenterDoiId.ToString())));

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts.AnyAsync(x => x.DomainOfInfluenceId == externalPrintingCenterDoiId)))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowIfNotAttachmentOwner()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfInvalidDomainOfInfluenceEntry()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(NewValidRequest(x =>
            {
                x.DomainOfInfluenceIds.Clear();
                x.DomainOfInfluenceIds.Add(DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenId);
            })),
            StatusCode.InvalidArgument,
            "Invalid domain of influence id found");
    }

    [Fact]
    public async Task ShouldThrowIfOwnerDomainOfInfluenceEntry()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(NewValidRequest(x =>
            {
                x.DomainOfInfluenceIds.Clear();
                x.DomainOfInfluenceIds.Add(DomainOfInfluenceMockData.ContestBundFutureApprovedBundId);
            })),
            StatusCode.InvalidArgument,
            "You cannot set the domain of influence attachment count of the owner");
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateDomainOfInfluenceAttachmentEntriesAsync(
                new()
                {
                    Id = AttachmentMockData.BundArchivedGemendeArneggId,
                    DomainOfInfluenceIds = { DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId },
                }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.UpdateDomainOfInfluenceAttachmentEntriesAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UpdateDomainOfInfluenceAttachmentEntriesRequest NewValidRequest(Action<UpdateDomainOfInfluenceAttachmentEntriesRequest>? customizer = null)
    {
        var request = new UpdateDomainOfInfluenceAttachmentEntriesRequest
        {
            Id = AttachmentMockData.BundFutureApprovedBund1Id,
            DomainOfInfluenceIds =
            {
                DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
                DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            },
        };
        customizer?.Invoke(request);
        return request;
    }
}
