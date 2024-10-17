// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AdditionalInvoicePositionTests;

public class ListAdditionalInvoicePositionsTest : BaseReadOnlyGrpcTest<AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient>
{
    public ListAdditionalInvoicePositionsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForPrintJobManagerShouldReturn()
    {
        var result = await AbraxasPrintJobManagerClient.ListAsync(new()
        { ContestId = ContestMockData.BundFutureApprovedId });
        result.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient service)
    {
        await service.ListAsync(new() { ContestId = ContestMockData.BundFutureApprovedId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }
}
