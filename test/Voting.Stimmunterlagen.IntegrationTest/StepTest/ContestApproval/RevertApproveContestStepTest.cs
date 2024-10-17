// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.ContestApproval;

public class RevertApproveContestStepTest : BaseReadOnlyStepTest
{
    public RevertApproveContestStepTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public Task ShouldThrow()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.RevertAsync(new RevertStepRequest
            {
                Step = Step.ContestApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
            }),
            StatusCode.InvalidArgument,
            "cannot revert ContestApproval");
    }
}
