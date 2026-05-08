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

public class UnassignPoliticalBusinessAttachmentTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    private static readonly Guid DefaultContestGuid = ContestMockData.BundFutureApprovedGuid;

    public UnassignPoliticalBusinessAttachmentTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<Data.Models.StepState>(
            x => x.DomainOfInfluence!.ContestId == DefaultContestGuid && x.Step == Data.Models.Step.PoliticalBusinessesApproval,
            x => x.Approved = true);
    }

    [Fact]
    public async Task ShouldRemovePoliticalBusinessFromAttachment()
    {
        await AbraxasElectionAdminClient.UnassignPoliticalBusinessAsync(new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedBund2Id,
            PoliticalBusinessId = VoteMockData.BundFutureApproved1Id,
        });

        var politicalBusinessCount = await RunOnDb(db => db.PoliticalBusinessAttachmentEntries
            .Where(x => x.AttachmentId == AttachmentMockData.BundFutureApprovedBund2Guid)
            .CountAsync());

        politicalBusinessCount.Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowIfNotPbAttendee()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(
                NewValidRequest(x => x.PoliticalBusinessId = VoteMockData.BundFuture6Id)),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundFutureApprovedBund1Id)),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldUnassignIfPbAttendeeAndDoiManager()
    {
        await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsId,
            PoliticalBusinessId = VoteMockData.BundFutureApproved1Id,
        });

        (await RunOnDb(db => db.PoliticalBusinessAttachmentEntries
            .Where(x => x.AttachmentId == AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsGuid)
            .AnyAsync())).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(
                new UnassignPoliticalBusinessAttachmentRequest
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
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfPoliticalBusinessesNotApproved()
    {
        await ModifyDbEntities<Data.Models.StepState>(
            x => x.DomainOfInfluence!.ContestId == DefaultContestGuid && x.Step == Data.Models.Step.PoliticalBusinessesApproval,
            x => x.Approved = false);

        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "The political businesses step is not approved yet.");
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.UnassignPoliticalBusinessAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UnassignPoliticalBusinessAttachmentRequest NewValidRequest(Action<UnassignPoliticalBusinessAttachmentRequest>? customizer = null)
    {
        var request = new UnassignPoliticalBusinessAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedGemeindeArneggId,
            PoliticalBusinessId = VoteMockData.BundFutureApprovedGemeindeArnegg1Id,
        };

        customizer?.Invoke(request);
        return request;
    }
}
