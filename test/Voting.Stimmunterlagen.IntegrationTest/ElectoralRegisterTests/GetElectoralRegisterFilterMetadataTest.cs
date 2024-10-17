// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Stimmregister;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ElectoralRegisterTests;

public class GetElectoralRegisterFilterMetadataTest : BaseReadOnlyGrpcTest<ElectoralRegisterService.ElectoralRegisterServiceClient>
{
    public GetElectoralRegisterFilterMetadataTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnMetadata()
    {
        var response = await StadtGossauElectionAdminClient.GetFilterMetadataAsync(NewValidRequest());
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public Task UnknownFilterIdShouldReturnNotFound()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.GetFilterMetadataAsync(NewValidRequest(x => x.FilterId = "f1689f64-64ef-4830-bef4-3753c0855abd")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ElectoralRegisterService.ElectoralRegisterServiceClient service)
        => await service.GetFilterMetadataAsync(NewValidRequest());

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static GetElectoralRegisterFilterMetadataRequest NewValidRequest(Action<GetElectoralRegisterFilterMetadataRequest>? customizer = null)
    {
        var request = new GetElectoralRegisterFilterMetadataRequest
        {
            FilterId = StimmregisterFilterMockData.SwissWithVotingRightId,
            Deadline = MockedClock.UtcNowTimestamp,
        };
        customizer?.Invoke(request);
        return request;
    }
}
