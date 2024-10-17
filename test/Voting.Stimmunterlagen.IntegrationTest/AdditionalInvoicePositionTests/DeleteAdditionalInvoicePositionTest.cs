// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;
using AdditionalInvoicePosition = Voting.Stimmunterlagen.Data.Models.AdditionalInvoicePosition;

namespace Voting.Stimmunterlagen.IntegrationTest.AdditionalInvoicePositionTests;

public class DeleteAdditionalInvoicePositionTest : BaseWriteableDbGrpcTest<AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient>
{
    public DeleteAdditionalInvoicePositionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldDelete()
    {
        var id = AdditionalInvoicePositionMockData.BundFutureApprovedArnegg1Id;
        await AbraxasPrintJobManagerClient.DeleteAsync(new()
        {
            Id = id,
        });

        var entity = await FindDbEntities<AdditionalInvoicePosition>(x => x.Id == Guid.Parse(id));
        entity.Any().Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowIfPositionNotExists()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.DeleteAsync(new()
            {
                Id = "324febcf-631e-4c4e-ada5-1845c96fbcd8",
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient service)
    {
        await service.DeleteAsync(new()
        {
            Id = AdditionalInvoicePositionMockData.BundFutureApprovedArnegg1Id,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }
}
