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

namespace Voting.Stimmunterlagen.IntegrationTest.PoliticalBusinessesTests;

public class ListPoliticalBusinessesTest : BaseReadOnlyGrpcTest<PoliticalBusinessService.PoliticalBusinessServiceClient>
{
    public ListPoliticalBusinessesTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnPoliticalBusinesses()
    {
        var result = await GemeindeArneggElectionAdminClient.ListAsync(new ListPoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnPoliticalBusinessesWithContestId()
    {
        var result = await GemeindeArneggElectionAdminClient.ListAsync(new ListPoliticalBusinessesRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowTenantWithoutAccess()
    {
        var businesses = await StaatskanzleiStGallenElectionAdminClient.ListAsync(new ListPoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureSchulgemeindeAndwilArneggId,
        });
        businesses.PoliticalBusinesses_.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(PoliticalBusinessService.PoliticalBusinessServiceClient service)
        => await service.ListAsync(new ListPoliticalBusinessesRequest { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId });

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
