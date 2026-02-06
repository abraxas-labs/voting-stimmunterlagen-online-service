// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using Step = Voting.Stimmunterlagen.Proto.V1.Models.Step;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.VoterListApproval;

public class RevertApproveVoterListsStepTest : BaseWriteableStepTest
{
    public RevertApproveVoterListsStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid;

        await ModifyDbEntities<Contest>(c => c.Id == ContestMockData.BundFutureGuid, c => c.GenerateVotingCardsDeadline = MockedClock.GetDate(100));

        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.Attachments, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.VoterLists, true);

        await StadtGossauElectionAdminClient.RevertAsync(new RevertStepRequest
        {
            Step = Step.VoterLists,
            DomainOfInfluenceId = doiGuid.ToString(),
        });

        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.VoterLists,
            false);
    }
}
