// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class GetContestDomainOfInfluenceVotingCardLayoutsTest
    : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public GetContestDomainOfInfluenceVotingCardLayoutsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var layouts = await AbraxasElectionAdminClient.GetContestLayoutsAsync(new() { ContestId = ContestMockData.BundFutureId });
        layouts.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyIfNotContestManager()
    {
        var layouts = await GemeindeArneggElectionAdminClient.GetContestLayoutsAsync(new() { ContestId = ContestMockData.BundFutureId });
        layouts.LayoutGroups.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.GetContestLayoutsAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
