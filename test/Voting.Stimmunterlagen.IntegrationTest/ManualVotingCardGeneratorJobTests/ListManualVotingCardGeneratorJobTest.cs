// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ManualVotingCardGeneratorJobTests;

public class ListManualVotingCardGeneratorJobTest : BaseReadOnlyGrpcTest<ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient>
{
    public ListManualVotingCardGeneratorJobTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var result = await GemeindeArneggElectionAdminClient.ListAsync(NewValidRequest());
        result.MatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient service)
    {
        await service.ListAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private ListManualVotingCardGeneratorJobsRequest NewValidRequest()
    {
        return new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId };
    }
}
