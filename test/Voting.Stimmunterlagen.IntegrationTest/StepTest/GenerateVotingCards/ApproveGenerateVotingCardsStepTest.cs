// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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

        await ModifyDbEntities<Data.Models.Attachment>(
            a => a.DomainOfInfluence!.ContestId == contestGuid,
            a => a.State = AttachmentState.Delivered);

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
                .Where(x => x.DomainOfInfluenceId == doiGuid)
                .ToListAsync());

        var swissJob = jobs.Single(x => x.Layout?.VotingCardType == VotingCardType.Swiss);
        swissJob.State.Should().Be(VotingCardGeneratorJobState.Ready);
        swissJob.CountOfVoters.Should().Be(5);
        swissJob.FileName.Should().Be("1240_Gemeinde_Arnegg_de.pdf");

        var eVotingJobs = jobs.Where(x => x.State == VotingCardGeneratorJobState.ReadyToRunOffline).ToList();
        eVotingJobs.All(x => x.State == VotingCardGeneratorJobState.ReadyToRunOffline).Should().BeTrue();
        eVotingJobs.All(x => x.CountOfVoters == 2).Should().BeTrue();
        eVotingJobs.Select(x => x.FileName)
            .OrderBy(x => x)
            .Should()
            .BeInAscendingOrder("de.pdf", "it.pdf");

        jobs.Should().HaveCount(3);

        var printJobs = await FindDbEntities<PrintJob>(p => p.DomainOfInfluence!.ContestId == contestGuid);

        // should only update print job state to ready for process if all attachments are delivered and generate voting cards is set.
        var affectedPrintJob = printJobs.Single(p => p.DomainOfInfluenceId == doiGuid).State.Should().Be(PrintJobState.ReadyForProcess);
        printJobs.Any(p => p.State != PrintJobState.ReadyForProcess).Should().BeTrue();
    }
}
