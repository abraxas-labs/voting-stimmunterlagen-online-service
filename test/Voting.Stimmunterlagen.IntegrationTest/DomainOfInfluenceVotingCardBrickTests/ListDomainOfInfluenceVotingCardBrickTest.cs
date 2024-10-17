// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardBrickTests;

public class ListDomainOfInfluenceVotingCardBrickTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient>
{
    public ListDomainOfInfluenceVotingCardBrickTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListShouldReturn()
    {
        var result = await AbraxasElectionAdminClient.ListAsync(NewValidRequest());
        result.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task NoBrickWithMatchingTemplateShouldReturnEmpty()
    {
        var result = await AbraxasElectionAdminClient.ListAsync(NewValidRequest(x => x.TemplateId = 2));
        result.Bricks.Should().HaveCount(0);
    }

    [Fact]
    public async Task NoBrickWithMatchingTenantShouldReturnEmpty()
    {
        var result = await StadtUzwilElectionAdminClient.ListAsync(NewValidRequest());
        result.Bricks.Should().HaveCount(0);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient service)
    {
        await service.ListAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static ListDomainOfInfluenceVotingCardBrickRequest NewValidRequest(Action<ListDomainOfInfluenceVotingCardBrickRequest>? customizer = null)
    {
        var req = new ListDomainOfInfluenceVotingCardBrickRequest
        {
            TemplateId = 1,
        };
        customizer?.Invoke(req);
        return req;
    }
}
