// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Database.Repositories;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using Step = Voting.Stimmunterlagen.Proto.V1.Models.Step;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.VoterListApproval;

public class ApproveVoterListsStepTest : BaseWriteableStepTest
{
    public ApproveVoterListsStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid;

        await ModifyDbEntities<Contest>(c => c.Id == ContestMockData.BundFutureGuid, c => c.GenerateVotingCardsDeadline = MockedClock.GetDate(100));

        // At least one voter list must be uploaded to approve.
        await RunScoped<IDbRepository<DataContext, VoterListImport>>(async r => await r.Create(new VoterListImport
        {
            DomainOfInfluenceId = doiGuid,
            Source = VoterListSource.ManualEch45Upload,
            SourceId = "test",
            VoterLists = new List<VoterList>
            {
                new VoterList { DomainOfInfluenceId = doiGuid },
            },
        }));

        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.Attachments, true);

        await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.VoterLists,
            DomainOfInfluenceId = doiGuid.ToString(),
        });
        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.VoterLists,
            true);
    }

    [Fact]
    public async Task NoVoterListShouldThrow()
    {
        await ModifyDbEntities<Contest>(c => c.Id == ContestMockData.BundFutureGuid, c => c.GenerateVotingCardsDeadline = MockedClock.GetDate(100));

        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid, Step.Attachments, true);

        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.VoterLists,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
            }),
            StatusCode.InvalidArgument,
            "Cannot approve voter lists step if no voter list are imported");
    }
}
