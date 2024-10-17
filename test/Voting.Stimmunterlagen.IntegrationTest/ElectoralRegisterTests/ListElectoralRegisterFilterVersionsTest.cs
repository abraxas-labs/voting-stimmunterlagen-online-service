// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ElectoralRegisterTests;

public class ListElectoralRegisterFilterVersionsTest : BaseReadOnlyGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public ListElectoralRegisterFilterVersionsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnFilterVersions()
    {
        var response = await StadtGossauElectionAdminClient.ListFilterVersionsAsync(new ListElectoralRegisterFilterVersionsRequest
        {
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
        });
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public Task UnknownFilterIdShouldReturnNotFound()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.ListFilterVersionsAsync(new ListElectoralRegisterFilterVersionsRequest { FilterId = "8234e8c9-aad3-4469-a58d-2ddb9cf83eb7" }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.ListFilterVersionsAsync(new ListElectoralRegisterFilterVersionsRequest
        {
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
        });

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
