// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class UpdateAttachmentTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public UpdateAttachmentTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        var id = AttachmentMockData.BundFutureApprovedGemeindeArneggGuid;
        await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest());
        var attachment = await RunOnDb(db => db.Attachments.SingleAsync(x => x.Id == id));
        var politicalBusinessIds = await RunOnDb(db => db.PoliticalBusinessAttachmentEntries
            .Where(x => x.AttachmentId == id)
            .Select(x => x.PoliticalBusinessId)
            .OrderBy(x => x)
            .ToListAsync());

        var count = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .Where(x => x.AttachmentId == id
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .SingleAsync());

        count.RequiredForVoterListsCount.Should().Be(5);

        attachment.ShouldMatchChildSnapshot("attachment");
        politicalBusinessIds.ShouldMatchChildSnapshot("politicalBusinessIds");
    }

    [Fact]
    public async Task ShouldUpdateInPoliticalAssembly()
    {
        var id = AttachmentMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid;
        await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest(x =>
        {
            x.Id = id.ToString();
            x.PoliticalBusinessIds.Clear();
        }));
        var attachment = await RunOnDb(db => db.Attachments.SingleAsync(x => x.Id == id));

        var count = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .Where(x => x.AttachmentId == id
                        && x.DomainOfInfluenceId == DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid)
            .SingleAsync());

        count.RequiredForVoterListsCount.Should().Be(3);

        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldUpdateIfDeliveryOnSameDayAsDeadline()
    {
        await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest(x => x.DeliveryPlannedOn = MockedClock.GetTimestampDate(15)));
        var attachment = await RunOnDb(db => db.Attachments.SingleAsync(x => x.Id == AttachmentMockData.BundFutureApprovedGemeindeArneggGuid));
        attachment.DeliveryPlannedOn.Should().Be(MockedClock.GetDate(15).Date);
    }

    [Fact]
    public async Task ShouldThrowIfNonZeroRequiredCountOnPoliticalParentType()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateAsync(NewValidRequest(x =>
            {
                x.Id = AttachmentMockData.BundFutureApprovedBund1Id;
                x.OrderedCount = 500;
                x.RequiredCount = 1;
                x.PoliticalBusinessIds.Clear();
                x.PoliticalBusinessIds.Add(VoteMockData.BundFutureApproved1Id);
            })),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Ch must have a ordered count greater than 0 and required count 0");
    }

    [Fact]
    public async Task ShouldThrowIfNonEqualCountOnNonPoliticalParentType()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest(x => x.OrderedCount = 1)),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Mu must have an equal ordered and required count and it has to be larger than 0");
    }

    [Fact]
    public async Task ShouldThrowIfNotPbAttendee()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest(x =>
            {
                x.PoliticalBusinessIds.Clear();
                x.PoliticalBusinessIds.Add(VoteMockData.BundFuture6Id);
            })),
            StatusCode.PermissionDenied,
            "only political businesses with permission entries allowed which are not on a domain of influence with external printing center");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(
                NewValidRequest(x =>
                {
                    x.Id = AttachmentMockData.BundArchivedGemendeArneggId;
                    x.PoliticalBusinessIds.Clear();
                    x.PoliticalBusinessIds.Add(VoteMockData.BundArchivedGemeindeArnegg1Id);
                })),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundFutureApprovedBund1Id)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfDeliveryDateAfterDeadline()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(
                NewValidRequest(x => x.DeliveryPlannedOn = MockedClock.GetTimestampDate(16))),
            StatusCode.InvalidArgument,
            "delivery deadline");
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAsync(NewValidRequest()),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldUpdateIfParentPb()
    {
        var id = AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsGuid;
        var req = NewValidParentPbRequest();
        await GemeindeArneggElectionAdminClient.UpdateAsync(req);

        var attachment = await RunOnDb(db => db.Attachments.SingleAsync(x => x.Id == id));
        var politicalBusinessIds = await RunOnDb(db => db.PoliticalBusinessAttachmentEntries
            .Where(x => x.AttachmentId == id)
            .Select(x => x.PoliticalBusinessId)
            .OrderBy(x => x)
            .ToListAsync());

        politicalBusinessIds
            .OrderBy(x => x)
            .Should()
            .BeEquivalentTo(req.PoliticalBusinessIds.Select(Guid.Parse).OrderBy(x => x));

        var count = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .Where(x => x.AttachmentId == id
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .SingleAsync());

        count.RequiredForVoterListsCount.Should().Be(9);
        count.RequiredCount.Should().Be(req.RequiredCount);

        attachment.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UpdateAttachmentRequest NewValidRequest(Action<UpdateAttachmentRequest>? customizer = null)
    {
        var request = new UpdateAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedGemeindeArneggId,
            Category = AttachmentCategory.BallotMu,
            Name = "Publikation Gossau updated",
            Format = AttachmentFormat.A6,
            Color = "Blue updated",
            Supplier = "Supplier updated",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(15),
            PoliticalBusinessIds =
                {
                    MajorityElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
                    ProportionalElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
                },
            OrderedCount = 4000,
            RequiredCount = 4000,
        };
        customizer?.Invoke(request);
        return request;
    }

    private static UpdateAttachmentRequest NewValidParentPbRequest(Action<UpdateAttachmentRequest>? customizer = null)
    {
        var request = new UpdateAttachmentRequest
        {
            Id = AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsId,
            Category = AttachmentCategory.BallotMu,
            Name = "Spezielles Arnegg Büchlein updated",
            Format = AttachmentFormat.A5,
            Color = "Blue",
            Supplier = "Lieferant",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(10),
            PoliticalBusinessIds =
                {
                    ProportionalElectionMockData.BundFutureApproved1Id,
                },
            OrderedCount = 2001,
            RequiredCount = 2001,
        };
        customizer?.Invoke(request);
        return request;
    }
}
