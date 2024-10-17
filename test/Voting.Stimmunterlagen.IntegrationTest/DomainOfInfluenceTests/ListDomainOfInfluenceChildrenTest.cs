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

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class ListDomainOfInfluenceChildrenTest : BaseReadOnlyGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public ListDomainOfInfluenceChildrenTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListBundChildrenShouldReturn()
    {
        var dois = await AbraxasElectionAdminClient.ListChildrenAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        dois.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListBundChildrenAsAttendeeShouldReturnOnlyReadable()
    {
        var dois = await GemeindeArneggElectionAdminClient.ListChildrenAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        dois.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceService.DomainOfInfluenceServiceClient service)
    {
        await service.ListManagedByCurrentTenantAsync(new ListDomainOfInfluencesRequest
        { ContestId = ContestMockData.BundFutureId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
