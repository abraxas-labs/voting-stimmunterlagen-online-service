// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class GetContestTest : BaseReadOnlyGrpcTest<ContestService.ContestServiceClient>
{
    public GetContestTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnContest()
    {
        var response = await StaatskanzleiStGallenElectionAdminClient.GetAsync(new() { Id = ContestMockData.BundFutureId });
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowTenantWithoutAccess()
    {
        await AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.GetAsync(new() { Id = ContestMockData.SchulgemeindeAndwilArneggFutureId }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
    {
        await service.GetAsync(new() { Id = ContestMockData.BundFutureId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
