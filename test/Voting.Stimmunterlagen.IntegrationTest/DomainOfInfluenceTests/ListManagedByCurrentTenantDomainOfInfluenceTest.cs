// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class ListManagedByCurrentTenantDomainOfInfluenceTest : BaseReadOnlyGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public ListManagedByCurrentTenantDomainOfInfluenceTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListManagedByCurrentTenantShouldReturn()
    {
        var dois = await StadtGossauElectionAdminClient.ListManagedByCurrentTenantAsync(new ListDomainOfInfluencesRequest
        { ContestId = ContestMockData.BundFutureApprovedId });

        var doi = dois.DomainOfInfluences_.Single();
        doi.ShortName.Should().Be("MU-GO");
    }

    [Fact]
    public async Task ListManagedByCurrentTenantShouldNotReturnAsContestManager()
    {
        var dois = await AbraxasElectionAdminClient.ListManagedByCurrentTenantAsync(new ListDomainOfInfluencesRequest
        { ContestId = ContestMockData.BundFutureId });

        var doi = dois.DomainOfInfluences_.Single();
        doi.ShortName.Should().Be("CH");
    }

    [Fact]
    public async Task ListManagedByCurrentTenantShouldNotReturnIfContestNotApproved()
    {
        var dois = await StadtGossauElectionAdminClient.ListManagedByCurrentTenantAsync(new ListDomainOfInfluencesRequest
        { ContestId = ContestMockData.BundFutureId });

        dois.DomainOfInfluences_.Should().BeEmpty();
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
