// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public class ListStepsTest : BaseReadOnlyGrpcTest<StepService.StepServiceClient>
{
    public ListStepsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturnSteps()
    {
        var steps = await StadtGossauElectionAdminClient.ListAsync(new ListStepsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        steps.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyStepsIfOtherTenant()
    {
        var steps = await StaatskanzleiStGallenElectionAdminClient.ListAsync(new ListStepsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        steps.Steps.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(StepService.StepServiceClient service)
    {
        await service.ListAsync(new ListStepsRequest { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId });
    }
}
