// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public class ApproveStepTest : BaseWriteableDbGrpcTest<StepService.StepServiceClient>
{
    public ApproveStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public Task ShouldFailIfStepIsAlreadyApproved()
    {
        return AssertStatus(
            async () => await StadtUzwilElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.PoliticalBusinessesApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilId,
            }),
            StatusCode.InvalidArgument,
            "step has a matching approved state already");
    }

    [Fact]
    public Task ShouldFailIfStepToApproveIsNotTheLatest()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.LayoutVotingCardsPoliticalBusinessAttendee,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "not all steps before LayoutVotingCardsPoliticalBusinessAttendee are approved");
    }

    [Fact]
    public async Task OtherTenantShouldThrowNotFound()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.PoliticalBusinessesApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ContestLockedShouldThrow()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.PoliticalBusinessesApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId,
            }),
            StatusCode.NotFound);
    }

    [Theory]
    [InlineData(Step.PoliticalBusinessesApproval)]
    [InlineData(Step.LayoutVotingCardsPoliticalBusinessAttendee)]
    [InlineData(Step.Attachments)]
    public async Task ContestPastSignUpDeadlineAndBelowStepVotingCardsShouldThrow(Step step)
    {
        await SetContestPrintingCenterSignUpDeadline(ContestMockData.BundFutureGuid, MockedClock.GetDate(-1));
        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = step,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "because it is past printing center sign up deadline");
    }

    [Theory]
    [InlineData(Step.VoterLists)]
    [InlineData(Step.GenerateVotingCards)]
    public async Task ContestPastGenerateVotingCardsDeadlineWithStepAboveVoterListsShouldThrow(Step step)
    {
        await SetContestGenerateVotingCardsDeadline(ContestMockData.BundFutureGuid, MockedClock.GetDate(-1));
        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = step,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "because it is past generate voting cards deadline");
    }

    protected override async Task AuthorizationTestCall(StepService.StepServiceClient service)
    {
        await service.ApproveAsync(new());
    }
}
