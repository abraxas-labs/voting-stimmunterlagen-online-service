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

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class ListDomainOfInfluencePoliticalBusinessAttendeesTest : BaseReadOnlyGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public ListDomainOfInfluencePoliticalBusinessAttendeesTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListBundAttendeesShouldReturn()
    {
        var dois = await AbraxasElectionAdminClient.ListPoliticalBusinessAttendeesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        dois.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListBundAttendeesAsAttendeeShouldReturnEmpty()
    {
        var dois = await GemeindeArneggElectionAdminClient.ListPoliticalBusinessAttendeesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        dois.DomainOfInfluences_.Count.Should().Be(0);
    }

    [Fact]
    public async Task ListSynodalwahlkreisArneggAttendeesShouldReturn()
    {
        var dois = await GemeindeArneggElectionAdminClient.ListPoliticalBusinessAttendeesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedSynodalwahlkreisArneggId });
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
