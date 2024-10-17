// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.AttachmentApproval;

public class RevertApproveAttachmentsStepTest : BaseWriteableStepTest
{
    public RevertApproveAttachmentsStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.Attachments,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.Attachments,
            true);

        await StadtGossauElectionAdminClient.RevertAsync(new RevertStepRequest
        {
            Step = Step.Attachments,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.Attachments,
            false);
    }
}
