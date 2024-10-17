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
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class GetDomainOfInfluenceVotingCardLayoutTemplateDataTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public GetDomainOfInfluenceVotingCardLayoutTemplateDataTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnValues()
    {
        var data = await GemeindeArneggElectionAdminClient.GetTemplateDataAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId });
        data.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyNoAccess()
    {
        var data = await StadtGossauElectionAdminClient.GetTemplateDataAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId });
        data.Containers.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.GetTemplateDataAsync(new() { DomainOfInfluenceId = DomainOfInfluenceMockData.GemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
