// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
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

public class CreateAdditionalInvoicePositionTest : BaseWriteableDbGrpcTest<AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient>
{
    public CreateAdditionalInvoicePositionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldCreate()
    {
        var response = await AbraxasPrintJobManagerClient.CreateAsync(NewValidRequest());
        var entity = await FindDbEntity<AdditionalInvoicePosition>(x => x.Id == Guid.Parse(response.Id));
        entity.Id = Guid.Empty;
        entity.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldCreateWithCommentIfEnabled()
    {
        var response = await AbraxasPrintJobManagerClient.CreateAsync(NewValidRequest(x =>
        {
            x.MaterialNumber = "1040.05.57";
            x.Comment = "Random comment";
        }));
        var entity = await FindDbEntity<AdditionalInvoicePosition>(x => x.Id == Guid.Parse(response.Id));
        entity.Comment.Should().Be("Random comment");
    }

    [Fact]
    public async Task ShouldThrowWithCommentIfDisabled()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x =>
                {
                    x.Comment = "Random comment";
                })),
            StatusCode.InvalidArgument,
            "Comment on material 1040.05.55 not enabled");
    }

    [Fact]
    public async Task ShouldThrowWithNoCommentIfRequired()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x =>
                {
                    x.MaterialNumber = "1040.05.57";
                })),
            StatusCode.InvalidArgument,
            "Comment on material 1040.05.57 is required");
    }

    [Fact]
    public async Task ShouldThrowIfNoPrintJobForDoiExists()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId)),
            StatusCode.InvalidArgument,
            "Cannot create or update an additional invoice position for a domain of influence if no print job exists");
    }

    [Fact]
    public async Task ShouldThrowIfInvalidMaterialNumber()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x => x.MaterialNumber = "Zusatzsticker")),
            StatusCode.InvalidArgument,
            "Material Zusatzsticker is not available");
    }

    [Fact]
    public async Task ShouldThrowIfNoAmount()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x => x.AmountCentime = 0)),
            StatusCode.InvalidArgument,
            "'AmountCentime' is smaller than the MinValue 25");
    }

    [Fact]
    public async Task ShouldThrowIfAmountNotDivisibleBy25()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(
                NewValidRequest(x => x.AmountCentime = 26)),
            StatusCode.InvalidArgument,
            "AmountCentime must be divisible by 25");
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            doi => doi.ExternalPrintingCenter = true);
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.CreateAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "Cannot create or update an additional invoice position for a domain of influence with an external printing center");
    }

    protected override async Task AuthorizationTestCall(AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient service)
    {
        await service.CreateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static CreateAdditionalInvoicePositionRequest NewValidRequest(Action<CreateAdditionalInvoicePositionRequest>? customizer = null)
    {
        var request = new CreateAdditionalInvoicePositionRequest
        {
            MaterialNumber = "1040.05.55",
            AmountCentime = 2525,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
        customizer?.Invoke(request);
        return request;
    }
}
