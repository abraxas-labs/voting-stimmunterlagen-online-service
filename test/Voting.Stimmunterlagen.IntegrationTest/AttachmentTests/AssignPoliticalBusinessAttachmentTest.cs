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

public class AssignPoliticalBusinessAttachmentTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public AssignPoliticalBusinessAttachmentTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldAddPoliticalBusinessToAttachment()
    {
        await StadtGossauElectionAdminClient.AssignPoliticalBusinessAsync(new AssignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedStadtGossauDeliveredId,
            PoliticalBusinessId = VoteMockData.BundFutureApprovedStadtGossau1Id,
        });

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.PoliticalBusinessEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid));
        attachment.PoliticalBusinessEntries.Should().HaveCount(2);
        attachment.PoliticalBusinessEntries!.Any(x => x.PoliticalBusinessId == VoteMockData.BundFutureApprovedStadtGossau1Guid).Should().BeTrue();

        attachment.TotalRequiredForVoterListsCount.Should().Be(4);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(4);
    }

    [Fact]
    public async Task ShouldThrowIfNotPbAttendee()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(
                NewValidRequest(x => x.PoliticalBusinessId = VoteMockData.BundFuture6Id)),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundFutureApprovedBund1Id)),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldAssignIfPbAttendeeAndDoiManager()
    {
        await StadtGossauElectionAdminClient.AssignPoliticalBusinessAsync(new AssignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedStadtGossauDeliveredId,
            PoliticalBusinessId = VoteMockData.BundFutureApproved1Id,
        });

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.PoliticalBusinessEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid));
        attachment.PoliticalBusinessEntries.Should().HaveCount(2);
        attachment.PoliticalBusinessEntries!.Any(x => x.PoliticalBusinessId == VoteMockData.BundFutureApproved1Guid).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowIfContestIsLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(
                new AssignPoliticalBusinessAttachmentRequest
                {
                    Id = AttachmentMockData.BundArchivedGemendeArneggId,
                    PoliticalBusinessId = VoteMockData.BundArchivedGemeindeArnegg1Id,
                }),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.PermissionDenied);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.AssignPoliticalBusinessAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static AssignPoliticalBusinessAttachmentRequest NewValidRequest(Action<AssignPoliticalBusinessAttachmentRequest>? customizer = null)
    {
        var request = new AssignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedGemeindeArneggId,
            PoliticalBusinessId = ProportionalElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
        };

        customizer?.Invoke(request);
        return request;
    }
}
