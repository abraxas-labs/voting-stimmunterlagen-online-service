// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using AdditionalInvoicePosition = Voting.Stimmunterlagen.Data.Models.AdditionalInvoicePosition;

namespace Voting.Stimmunterlagen.IntegrationTest.AdditionalInvoicePositionTests;

public class UpdateAdditionalInvoicePositionTest : BaseWriteableDbGrpcTest<AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient>
{
    public UpdateAdditionalInvoicePositionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldUpdate()
    {
        var req = NewValidRequest();
        await AbraxasPrintJobManagerClient.UpdateAsync(req);
        var entity = await FindDbEntity<AdditionalInvoicePosition>(x => x.Id == Guid.Parse(req.Id));
        entity.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfPositionNotExists()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateAsync(
                NewValidRequest(x => x.Id = "324febcf-631e-4c4e-ada5-1845c96fbcd8")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNoPrintJobForDoiExists()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId)),
            StatusCode.InvalidArgument,
            "Cannot create or update an additional invoice position for a domain of influence if no print job exists");
    }

    [Fact]
    public async Task ShouldThrowIfInvalidMaterialNumber()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateAsync(
                NewValidRequest(x => x.MaterialNumber = "Zusatzsticker2")),
            StatusCode.InvalidArgument,
            "Material Zusatzsticker2 is not available");
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
            doi => doi.ExternalPrintingCenter = true);
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.UpdateAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "Cannot create or update an additional invoice position for a domain of influence with an external printing center");
    }

    protected override async Task AuthorizationTestCall(AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient service)
    {
        await service.UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static UpdateAdditionalInvoicePositionRequest NewValidRequest(Action<UpdateAdditionalInvoicePositionRequest>? customizer = null)
    {
        var request = new UpdateAdditionalInvoicePositionRequest
        {
            Id = AdditionalInvoicePositionMockData.BundFutureApprovedArnegg1Id,
            MaterialNumber = "1040.05.55",
            AmountCentime = 10100,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
        };
        customizer?.Invoke(request);
        return request;
    }
}
