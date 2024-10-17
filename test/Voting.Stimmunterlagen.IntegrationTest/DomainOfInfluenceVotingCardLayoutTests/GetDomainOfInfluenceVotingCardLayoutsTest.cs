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

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class GetDomainOfInfluenceVotingCardLayoutsTest
    : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public GetDomainOfInfluenceVotingCardLayoutsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWorkByDomainOfInfluence()
    {
        var layouts = await GemeindeArneggElectionAdminClient.GetLayoutsAsync(new GetDomainOfInfluenceVotingCardLayoutsRequest
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
        layouts.MatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.GetLayoutsAsync(new GetDomainOfInfluenceVotingCardLayoutsRequest
        { DomainOfInfluenceId = DomainOfInfluenceMockData.GemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
