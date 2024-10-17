// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.VoterListsApproval;

public class ApproveVoterListsStepTest : BaseWriteableStepTest
{
    public ApproveVoterListsStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.Attachments, true);
        await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.VoterLists,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
        });

        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
            Step.VoterLists,
            true);
    }

    [Fact]
    public async Task ShouldThrowIfRequiredCountAsAnAttendeeNotSet()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, Step.Attachments, true);

        await RunOnDb(async db =>
        {
            var voterList = await db.VoterLists
                    .AsTracking()
                    .SingleAsync(vl => vl.Id == VoterListMockData.BundFutureApprovedStadtGossauSwissGuid);

            voterList.HasVoterDuplicates = true;
            await db.SaveChangesAsync();
        });

        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.VoterLists,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "Cannot complete the step while uploaded voter duplicates exists");
    }
}
