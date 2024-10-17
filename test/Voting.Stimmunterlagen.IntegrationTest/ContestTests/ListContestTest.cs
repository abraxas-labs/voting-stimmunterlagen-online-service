// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class ListContestTest : BaseReadOnlyGrpcTest<ContestService.ContestServiceClient>
{
    public ListContestTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var response = await AbraxasElectionAdminClient.ListAsync(new());

        foreach (var contest in response.Contests_)
        {
            contest.DomainOfInfluence.Should().NotBeNull();
            contest.DomainOfInfluence = null;
        }

        response.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldWorkWithStatesFilter()
    {
        var response = await AbraxasElectionAdminClient.ListAsync(new()
        {
            States =
                {
                    ContestState.Archived,
                },
        });

        response.Contests_.All(x => x.State == ContestState.Archived).Should().BeTrue();
        response.Contests_.Should().HaveCount(2);
    }

    [Fact]
    public async Task UnknownTenantShouldReturnEmpty()
    {
        var response = await UnknownClient.ListAsync(new());
        response.Contests_.Should().HaveCount(0);
    }

    [Fact]
    public async Task OtherTenantShouldReturnAll()
    {
        var response = await GemeindeArneggElectionAdminClient.ListAsync(new());
        response.Contests_.Select(x => x.Id).Should().Contain(ContestMockData.SchulgemeindeAndwilArneggFutureId);
    }

    [Fact]
    public async Task PrintJobManagerShouldReturnAll()
    {
        var response = await GemeindeArneggPrintJobManagerClient.ListAsync(new());

        foreach (var contest in response.Contests_)
        {
            contest.DomainOfInfluence = null;
        }

        response.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
        => await service.ListAsync(new());
}
