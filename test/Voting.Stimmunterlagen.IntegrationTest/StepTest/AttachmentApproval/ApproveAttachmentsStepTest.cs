// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using DomainOfInfluenceAttachmentCount = Voting.Stimmunterlagen.Data.Models.DomainOfInfluenceAttachmentCount;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.AttachmentApproval;

public class ApproveAttachmentsStepTest : BaseWriteableStepTest
{
    public ApproveAttachmentsStepTest(TestApplicationFactory factory)
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
            Step.PoliticalBusinessesApproval,
            true);
    }

    [Fact]
    public async Task ShouldThrowIfRequiredCountAsAnAttendeeNotSet()
    {
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);

        await RunOnDb(async db =>
        {
            db.Attachments.Add(new()
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
                DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>()
                {
                    new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid },
                },
            });
            await db.SaveChangesAsync();
        });

        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.Attachments,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "required count in attachments as attendee not set");
    }
}
