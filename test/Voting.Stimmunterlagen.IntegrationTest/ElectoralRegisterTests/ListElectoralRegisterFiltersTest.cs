// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Snapper;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ElectoralRegisterTests;

public class ListElectoralRegisterFiltersTest : BaseReadOnlyGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public ListElectoralRegisterFiltersTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnFilters()
    {
        var response = await StadtGossauElectionAdminClient.ListFiltersAsync(ProtobufEmpty.Instance);
        response.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.ListFiltersAsync(ProtobufEmpty.Instance);

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
