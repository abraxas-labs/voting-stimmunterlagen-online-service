// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListTests;

public class ListVoterListsTest : BaseReadOnlyGrpcTest<VoterListService.VoterListServiceClient>
{
    public ListVoterListsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListShouldWork()
    {
        var voterLists = await GemeindeArneggElectionAdminClient.ListAsync(new ListVoterListsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });

        voterLists.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(VoterListService.VoterListServiceClient service)
    {
        await service.ListAsync(new ListVoterListsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
