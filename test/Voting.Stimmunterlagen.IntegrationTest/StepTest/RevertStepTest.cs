// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest;

public class RevertStepTest : BaseReadOnlyGrpcTest<StepService.StepServiceClient>
{
    public RevertStepTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public Task ShouldFailIfStepIsNotApproved()
    {
        return AssertStatus(
            async () => await StadtUzwilElectionAdminClient.RevertAsync(new RevertStepRequest
            {
                Step = Step.LayoutVotingCardsPoliticalBusinessAttendee,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilId,
            }),
            StatusCode.InvalidArgument,
            "step has a matching approved state already");
    }

    [Fact]
    public Task ShouldFailIfStepToRevertIsNotTheLatest()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.RevertAsync(new RevertStepRequest
            {
                Step = Step.PoliticalBusinessesApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
            }),
            StatusCode.InvalidArgument,
            "only the last approved step can be reverted");
    }

    [Fact]
    public async Task OtherTenantShouldThrowNotFound()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.RevertAsync(new RevertStepRequest
            {
                Step = Step.PoliticalBusinessesApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(StepService.StepServiceClient service)
    {
        await service.RevertAsync(new());
    }
}
