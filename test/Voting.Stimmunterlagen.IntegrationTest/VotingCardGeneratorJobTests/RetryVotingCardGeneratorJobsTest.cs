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
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using VotingCardGeneratorJob = Voting.Stimmunterlagen.Data.Models.VotingCardGeneratorJob;
using VotingCardGeneratorJobState = Voting.Stimmunterlagen.Data.Models.VotingCardGeneratorJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardGeneratorJobTests;

public class RetryVotingCardGeneratorJobsTest : BaseWriteableDbGrpcTest<
    VotingCardGeneratorJobsService.VotingCardGeneratorJobsServiceClient>
{
    public RetryVotingCardGeneratorJobsTest(TestApplicationFactory factory)
        : base(factory)
    {
        GetService<VotingCardGeneratorThrottlerMock>().ShouldBlock = true;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<VotingCardGeneratorJob>(
            x => x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid,
            x => x.State = VotingCardGeneratorJobState.Failed);
        await ModifyDbEntities<VotingCardGeneratorJob>(
            x => x.Id == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob2Guid,
            x => x.State = VotingCardGeneratorJobState.Completed);
    }

    [Fact]
    public async Task ShouldSetStatesReady()
    {
        await GemeindeArneggElectionAdminClient.RetryJobsAsync(NewValidRequest());

        var jobs = await FindDbEntities<VotingCardGeneratorJob>(x =>
            x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        jobs.Should().HaveCount(4);
        jobs.Count(x => x.State == VotingCardGeneratorJobState.Ready).Should().Be(2);
        jobs.Count(x => x.State == VotingCardGeneratorJobState.ReadyToRunOffline).Should().Be(1);
        jobs.Count(x => x.State == VotingCardGeneratorJobState.Completed).Should().Be(1);
        GetService<VotingCardGeneratorThrottlerMock>().BlockedCount.Should().Be(2);
    }

    [Fact]
    public async Task ShouldNotUpdateStateIfNotInTestingPhase()
    {
        await SetContestState(ContestMockData.BundFutureApprovedGuid, ContestState.Active);

        await GemeindeArneggElectionAdminClient.RetryJobsAsync(NewValidRequest());

        var jobs = await FindDbEntities<VotingCardGeneratorJob>(x =>
            x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        jobs.Should().HaveCount(4);
        jobs.Count(x => x.State == VotingCardGeneratorJobState.Ready).Should().Be(1);
    }

    protected override async Task AuthorizationTestCall(VotingCardGeneratorJobsService.VotingCardGeneratorJobsServiceClient service)
    {
        await service.RetryJobsAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private RetryVotingCardGeneratorJobsRequest NewValidRequest()
    {
        return new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
    }
}
