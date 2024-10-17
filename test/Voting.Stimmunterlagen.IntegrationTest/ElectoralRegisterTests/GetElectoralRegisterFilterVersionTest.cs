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

public class GetElectoralRegisterFilterVersionTest : BaseReadOnlyGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public GetElectoralRegisterFilterVersionTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnFilterVersion()
    {
        var response = await StadtGossauElectionAdminClient.GetFilterVersionAsync(new GetElectoralRegisterFilterVersionRequest
        {
            FilterVersionId = StimmregisterFilterMockData.SwissWithVotingRightVersion1Id,
        });
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public Task UnknownFilterVersionIdShouldReturnNotFound()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.GetFilterVersionAsync(new GetElectoralRegisterFilterVersionRequest { FilterVersionId = "e213bfaa-07f9-4af7-b1ff-ae89aa5546d6" }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.GetFilterVersionAsync(new GetElectoralRegisterFilterVersionRequest { FilterVersionId = StimmregisterFilterMockData.SwissWithVotingRightVersion1Id });

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
