// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class SetAttachmentStationTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public SetAttachmentStationTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSetStation()
    {
        await AbraxasPrintJobManagerClient.SetStationAsync(NewValidRequest());

        var attachment = await RunOnDb(db => db.Attachments
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid));

        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldSetStationIfSameStationOnSameLevelButDifferentTenant()
    {
        await AbraxasPrintJobManagerClient.SetStationAsync(NewValidRequest(x =>
        {
            x.Id = AttachmentMockData.BundFutureApprovedStadtGossauDeliveredId;
            x.Station = 31;
        }));

        var attachment = await RunOnDb(db => db.Attachments
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid));

        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetStationAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundArchivedGemendeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.SetStationAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.SetStationAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static SetAttachmentStationRequest NewValidRequest(Action<SetAttachmentStationRequest>? customizer = null)
    {
        var request = new SetAttachmentStationRequest
        {
            Id = AttachmentMockData.BundFutureApprovedKantonStGallenId,
            Station = 19,
        };

        customizer?.Invoke(request);
        return request;
    }
}
