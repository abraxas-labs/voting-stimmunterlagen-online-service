// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class GetDomainOfInfluenceTest : BaseReadOnlyGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public GetDomainOfInfluenceTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnDomainOfInfluence()
    {
        var response = await StaatskanzleiStGallenElectionAdminClient.GetAsync(new()
        {
            Id = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnDomainOfInfluenceWithAllow()
    {
        var response = await GemeindeArneggElectionAdminClient.GetAsync(new()
        {
            Id = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
        });
        response.AllowManualVoterListUpload.Should().BeTrue();
        response.CanManuallyUploadVoterList.Should().BeTrue();
        response.ElectoralRegistrationEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowTenantWithoutAccess()
    {
        await AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.GetAsync(new()
            {
                Id = DomainOfInfluenceMockData.ContestBundFutureSchulgemeindeAndwilArneggId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceService.DomainOfInfluenceServiceClient service)
    {
        await service.GetAsync(new() { Id = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
