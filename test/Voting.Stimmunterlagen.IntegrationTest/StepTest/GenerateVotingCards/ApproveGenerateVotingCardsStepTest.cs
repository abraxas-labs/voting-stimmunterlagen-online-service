// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using AttachmentState = Voting.Stimmunterlagen.Data.Models.AttachmentState;
using PrintJob = Voting.Stimmunterlagen.Data.Models.PrintJob;
using PrintJobState = Voting.Stimmunterlagen.Data.Models.PrintJobState;
using Step = Voting.Stimmunterlagen.Proto.V1.Models.Step;
using VotingCardGeneratorJobState = Voting.Stimmunterlagen.Data.Models.VotingCardGeneratorJobState;
using VotingCardType = Voting.Stimmunterlagen.Data.Models.VotingCardType;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.GenerateVotingCards;

public class ApproveGenerateVotingCardsStepTest : BaseWriteableStepTest
{
    public ApproveGenerateVotingCardsStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var contestGuid = ContestMockData.BundFutureApprovedGuid;
        var doiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId;
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

        await ModifyDbEntities<Attachment>(
            a => a.DomainOfInfluence!.ContestId == contestGuid,
            a => a.State = AttachmentState.Delivered);

        await ModifyDbEntities<Voter>(
            v => v.PersonId == "1",
            v => v.VotingCardPrintDisabled = true);

        GetService<VotingCardGeneratorThrottlerMock>().ShouldBlock = true;
        await SetStepApproved(doiGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(doiGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(doiGuid, Step.Attachments, true);
        await SetStepApproved(doiGuid, Step.VoterLists, true);
        await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.GenerateVotingCards,
            DomainOfInfluenceId = doiId,
        });
        await AssertStepApproved(
            doiGuid,
            Step.GenerateVotingCards,
            true);

        var doi = await FindDbEntity<ContestDomainOfInfluence>(x =>
            x.Id == doiGuid);
        doi.GenerateVotingCardsTriggered.Should().Be(MockedClock.UtcNowDate);

        var jobs = await RunOnDb(db =>
            db.VotingCardGeneratorJobs
                .Include(x => x.Layout)
                .Include(x => x.Voter)
                .Where(x => x.DomainOfInfluenceId == doiGuid)
                .ToListAsync());

        var voters = jobs.SelectMany(j => j.Voter).ToList();
        voters.Count.Should().Be(8);
        voters.All(v => !v.VotingCardPrintDisabled).Should().BeTrue();

        var swissJob = jobs.Single(x => x.Layout?.VotingCardType == VotingCardType.Swiss && !x.HasEmptyVotingCards);
        swissJob.State.Should().Be(VotingCardGeneratorJobState.Ready);
        swissJob.CountOfVoters.Should().Be(4);
        swissJob.FileName.Should().Be("U_1240_Gemeinde_Arnegg_1234_de_20200112.pdf");

        var swissJobEmpty = jobs.Single(x => x.Layout?.VotingCardType == VotingCardType.Swiss && x.HasEmptyVotingCards);
        swissJobEmpty.State.Should().Be(VotingCardGeneratorJobState.Ready);
        swissJobEmpty.CountOfVoters.Should().Be(2);
        swissJobEmpty.FileName.Should().Be("U_1240_Gemeinde_Arnegg_EMPTY_20200112.pdf");

        var eVotingJobs = jobs.Where(x => x.State == VotingCardGeneratorJobState.ReadyToRunOffline).ToList();
        eVotingJobs.All(x => x.State == VotingCardGeneratorJobState.ReadyToRunOffline).Should().BeTrue();
        eVotingJobs.All(x => x.CountOfVoters == 2).Should().BeTrue();
        eVotingJobs.Select(x => x.FileName)
            .OrderBy(x => x)
            .Should()
            .BeInAscendingOrder("de.pdf", "it.pdf");

        eVotingJobs.Any(x => x.FileName == "U_1240_Gemeinde_Arnegg_EVoting_1234_de_20200112.pdf")
            .Should()
            .BeTrue();

        jobs.Should().HaveCount(4);

        var printJobs = await FindDbEntities<PrintJob>(p => p.DomainOfInfluence!.ContestId == contestGuid);

        // should only update print job state to ready for process if all attachments are delivered and generate voting cards is set.
        var affectedPrintJob = printJobs.Single(p => p.DomainOfInfluenceId == doiGuid).State.Should().Be(PrintJobState.ReadyForProcess);
        printJobs.Any(p => p.State != PrintJobState.ReadyForProcess).Should().BeTrue();
    }

    [Fact]
    public async Task EVotingNotApprovedShouldThrow()
    {
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

        await ModifyDbEntities<ContestCountingCircle>(
            cc => cc.Id == CountingCircleMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            cc => cc.EVoting = true);

        await ModifyDbEntities<PoliticalBusiness>(
            pb => pb.Id == VoteMockData.BundFutureApprovedGemeindeArnegg1Guid,
            pb => pb.EVotingApproved = false);

        await SetStepApproved(doiGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(doiGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(doiGuid, Step.Attachments, true);
        await SetStepApproved(doiGuid, Step.VoterLists, true);

        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
            {
                Step = Step.GenerateVotingCards,
                DomainOfInfluenceId = doiGuid.ToString(),
            }),
            StatusCode.InvalidArgument,
            "Political business a9ffc699-3542-45df-8bac-febea9f60c1a has not approved e-voting");
    }

    [Fact]
    public async Task EVotingApprovedShouldWork()
    {
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

        await ModifyDbEntities<ContestCountingCircle>(
            cc => cc.Id == CountingCircleMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            cc => cc.EVoting = true);

        await ModifyDbEntities<PoliticalBusiness>(
            pb => pb.Id == VoteMockData.BundFutureApprovedGemeindeArnegg1Guid,
            pb => pb.EVotingApproved = true);

        // Should work, if a non related political business of the domain of influence is still not approved.
        await ModifyDbEntities<PoliticalBusiness>(
            pb => pb.Id == VoteMockData.BundFutureApprovedStadtUzwil1Guid,
            pb => pb.EVotingApproved = false);

        await SetStepApproved(doiGuid, Step.PoliticalBusinessesApproval, true);
        await SetStepApproved(doiGuid, Step.LayoutVotingCardsPoliticalBusinessAttendee, true);
        await SetStepApproved(doiGuid, Step.Attachments, true);
        await SetStepApproved(doiGuid, Step.VoterLists, true);

        await GemeindeArneggElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.GenerateVotingCards,
            DomainOfInfluenceId = doiGuid.ToString(),
        });
        await AssertStepApproved(
            doiGuid,
            Step.GenerateVotingCards,
            true);
    }
}
