// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AdditionalInvoicePositionTests;

public class ListAdditionalInvoicePositionAvailableMaterialsTest : BaseReadOnlyGrpcTest<AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient>
{
    public ListAdditionalInvoicePositionAvailableMaterialsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForPrintJobManagerShouldReturn()
    {
        var result = await AbraxasPrintJobManagerClient.ListAvailableMaterialAsync(new());
        result.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(AdditionalInvoicePositionService.AdditionalInvoicePositionServiceClient service)
    {
        await service.ListAvailableMaterialAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }
}
