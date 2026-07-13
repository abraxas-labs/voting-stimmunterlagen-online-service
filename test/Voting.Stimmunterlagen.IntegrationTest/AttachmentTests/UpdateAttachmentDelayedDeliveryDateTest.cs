// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class UpdateAttachmentDelayedDeliveryDateTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public UpdateAttachmentDelayedDeliveryDateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<Attachment>(
            x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid,
            x => x.DeliveryPlannedOn = MockedClock.GetDate(0));
    }

    [Fact]
    public async Task ShouldSetDate()
    {
        await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(NewValidRequest());

        var attachment = await RunOnDb(db => db.Attachments
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid));

        attachment.DelayedDeliveryDate.Should().Be(MockedClock.GetDate(1).Date);
    }

    [Fact]
    public async Task ShouldUnsetDate()
    {
        var request = NewValidRequest(x => x.DelayedDeliveryDate = null);

        await ModifyDbEntities<Attachment>(
            x => x.Id == Guid.Parse(request.Id),
            x => x.DelayedDeliveryDate = MockedClock.GetDate(5));

        await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(request);

        var attachment = await RunOnDb(db => db.Attachments
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid));

        attachment.DelayedDeliveryDate.Should().Be(null);
    }

    [Fact]
    public async Task ShouldThrowIfSameDateAsPlannedDelivery()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(
                NewValidRequest(x => x.DelayedDeliveryDate = MockedClock.GetTimestampDate(0))),
            StatusCode.InvalidArgument,
            "Delayed delivery date must happen after the planned delivery date");
    }

    [Fact]
    public async Task ShouldThrowIfBeforePlannedDelivery()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(
                NewValidRequest(x => x.DelayedDeliveryDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Delayed delivery date must happen after the planned delivery date");
    }

    [Fact]
    public async Task ShouldThrowIfAlreadyDelivered()
    {
        var request = NewValidRequest();
        await ModifyDbEntities<Attachment>(
            x => x.Id == Guid.Parse(request.Id),
            x => x.State = AttachmentState.Delivered);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(request),
            StatusCode.InvalidArgument,
            "Cannot set a delayed delivery date if already delivered");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundArchivedGemendeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
            x => x.ExternalPrintingCenter = true);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateDelayedDeliveryDateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.UpdateDelayedDeliveryDateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static UpdateAttachmentDelayedDeliveryDateRequest NewValidRequest(Action<UpdateAttachmentDelayedDeliveryDateRequest>? customizer = null)
    {
        var request = new UpdateAttachmentDelayedDeliveryDateRequest
        {
            Id = AttachmentMockData.BundFutureApprovedKantonStGallenId,
            DelayedDeliveryDate = MockedClock.GetTimestampDate(1),
        };

        customizer?.Invoke(request);
        return request;
    }
}
