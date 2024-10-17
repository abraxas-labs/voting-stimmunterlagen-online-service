// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class GetAttachmentsProgressTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public GetAttachmentsProgressTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnTrueWithAllStationsSet()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtGossauGuid,
            x => x.EVoting = true);

        // set all stations of the contest, except attachment "Gossau Umschlag".
        await ModifyDbEntities<Data.Models.Attachment>(
            x => x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid && x.Id != AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid,
            x => x.Station = 1);

        await ModifyDbEntities<Data.Models.DomainOfInfluenceAttachmentCount>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
            x => x.RequiredCount = 1);

        // set required counts for all dois of "Gossau Umschlag" to 0, indicating that the e-voting dois explicitly dont need this attachment.
        await ModifyDbEntities<Data.Models.DomainOfInfluenceAttachmentCount>(
            x => x.AttachmentId == AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid,
            x => x.RequiredCount = 0);

        var result = await AbraxasElectionAdminClient.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
        result.StationsSet.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnFalseWithNotAllStationsSet()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtGossauGuid,
            x => x.EVoting = true);

        var result = await AbraxasElectionAdminClient.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
        result.StationsSet.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnFalseWithMissingRequiredCounts()
    {
        await ModifyDbEntities<ContestCountingCircle>(
            x => x.Id == CountingCircleMockData.ContestBundFutureApprovedStadtGossauGuid,
            x => x.EVoting = true);

        // set all stations of the contest, except attachment "Gossau Umschlag".
        await ModifyDbEntities<Data.Models.Attachment>(
            x => x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid && x.Id != AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid,
            x => x.Station = 1);

        await ModifyDbEntities<Data.Models.DomainOfInfluenceAttachmentCount>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
            x => x.RequiredCount = 1);

        // set required counts for all dois of "Gossau Umschlag" to null, indicating that the doi has not chosen yet.
        await ModifyDbEntities<Data.Models.DomainOfInfluenceAttachmentCount>(
            x => x.AttachmentId == AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid,
            x => x.RequiredCount = null);

        var result = await AbraxasElectionAdminClient.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
        result.StationsSet.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnTrueWithNoAttachments()
    {
        await RunOnDb(async db =>
        {
            var attachments = await db.Attachments.ToListAsync();
            db.Attachments.RemoveRange(attachments);
            await db.SaveChangesAsync();
        });

        var result = await AbraxasElectionAdminClient.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
        result.StationsSet.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowIfNotContestManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId }),
            StatusCode.PermissionDenied);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.GetAttachmentsProgressAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
