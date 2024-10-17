// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestEVotingExportJobTests;

public class GetContestEVotingExportJobTest : BaseReadOnlyGrpcTest<
    ContestEVotingExportJobService.ContestEVotingExportJobServiceClient>
{
    private const string DefaultContestId = ContestMockData.BundFutureApprovedId;

    public GetContestEVotingExportJobTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnForContestManager()
    {
        var job = await AbraxasElectionAdminClient.GetJobAsync(new()
        {
            ContestId = DefaultContestId,
        });
        job.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowForNonContestManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetJobAsync(new()
            {
                ContestId = DefaultContestId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestEVotingExportJobService.ContestEVotingExportJobServiceClient service)
    {
        await service.GetJobAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
