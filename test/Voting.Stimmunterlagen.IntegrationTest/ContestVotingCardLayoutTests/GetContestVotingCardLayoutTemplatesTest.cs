// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestVotingCardLayoutTests;

public class GetContestVotingCardLayoutTemplatesTest : BaseReadOnlyGrpcTest<ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient>
{
    public GetContestVotingCardLayoutTemplatesTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var templates = await AbraxasElectionAdminClient.GetTemplatesAsync(new GetContestVotingCardLayoutTemplatesRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });
        templates.ShouldMatchSnapshot();
    }

    [Fact]
    public Task ShouldReturnPermissionDeniedIfNotContestManager()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetTemplatesAsync(new GetContestVotingCardLayoutTemplatesRequest
            {
                ContestId = ContestMockData.BundFutureId,
            }),
            StatusCode.PermissionDenied);
    }

    protected override async Task AuthorizationTestCall(ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient service)
    {
        await service.GetTemplatesAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
