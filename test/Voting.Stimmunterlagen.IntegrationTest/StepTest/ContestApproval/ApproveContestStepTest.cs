// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.ContestApproval;

public class ApproveContestStepTest : BaseWriteableStepTest
{
    public ApproveContestStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.ContestApproval, false);
        await AbraxasElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.ContestApproval,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
        });
        await AssertStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.ContestApproval, true);
    }

    [Fact]
    public async Task ShouldThrowIfDeadlinesNotSet()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.ContestApproval, false);
        await ModifyDbEntities<Contest>(x => x.Id == ContestMockData.BundFutureApprovedGuid, x =>
        {
            x.AttachmentDeliveryDeadline = null;
            x.PrintingCenterSignUpDeadline = null;
            x.Approved = null;
        });

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.ContestApproval,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
            }),
            StatusCode.InvalidArgument,
            "deadlines need to be specified to approve the contest");
    }
}
