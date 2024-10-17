// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestVotingCardLayoutTests;

public class GetContestVotingCardLayoutsTest : BaseReadOnlyGrpcTest<ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient>
{
    public GetContestVotingCardLayoutsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var layouts = await AbraxasElectionAdminClient.GetLayoutsAsync(new GetContestVotingCardLayoutsRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });
        layouts.Layouts.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyIfNotContestManager()
    {
        var layouts = await GemeindeArneggElectionAdminClient.GetLayoutsAsync(new GetContestVotingCardLayoutsRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });
        layouts.Layouts.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient service)
    {
        await service.GetLayoutsAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
