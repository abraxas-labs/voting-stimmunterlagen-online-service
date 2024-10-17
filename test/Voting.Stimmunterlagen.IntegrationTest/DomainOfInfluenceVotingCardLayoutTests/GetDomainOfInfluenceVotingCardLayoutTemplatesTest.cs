// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class GetDomainOfInfluenceVotingCardLayoutTemplatesTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public GetDomainOfInfluenceVotingCardLayoutTemplatesTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var templates = await AbraxasElectionAdminClient.GetTemplatesAsync(new GetDomainOfInfluenceVotingCardLayoutTemplatesRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });
        templates.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldWorkAsAttendee()
    {
        var templates = await GemeindeArneggElectionAdminClient.GetTemplatesAsync(new GetDomainOfInfluenceVotingCardLayoutTemplatesRequest
        {
            ContestId = ContestMockData.BundFutureId,
        });

        templates.Templates_
            .Select(t => t.Name)
            .Should()
            .BeEquivalentTo(
                "template-001-swiss",
                "template-002-evoting",
                "template-003-others",
                "template-004-others2",
                "template-100-swiss-arnegg",
                "template-101-swiss-arnegg",
                "template-800-swiss-arnegg");
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.GetTemplatesAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
