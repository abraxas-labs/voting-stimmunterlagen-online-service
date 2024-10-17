// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public class ImmutableStepTest : BaseReadOnlyStepTest
{
    public ImmutableStepTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData(Step.PrintJobOverview)]
    [InlineData(Step.GenerateManualVotingCards)]
    [InlineData(Step.ContestOverview)]
    [InlineData(Step.VotingJournal)]
    public Task ApproveShouldThrow(Step step)
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = step,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            }),
            StatusCode.InvalidArgument,
            $"the step {step} is not directly mutable");
    }

    [Theory]
    [InlineData(Step.PrintJobOverview)]
    [InlineData(Step.GenerateManualVotingCards)]
    [InlineData(Step.ContestOverview)]
    [InlineData(Step.VotingJournal)]
    public Task RevertShouldThrow(Step step)
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.RevertAsync(new RevertStepRequest
            {
                Step = step,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            }),
            StatusCode.InvalidArgument,
            $"the step {step} is not directly mutable");
    }
}
