// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class ListPrintJobSummariesTest : BaseWriteableDbGrpcTest<PrintJobService.PrintJobServiceClient>
{
    public ListPrintJobSummariesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForContest()
    {
        var printJobs = await AbraxasPrintJobManagerClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListFilterDomainOfInfluenceName()
    {
        var printJobs = await AbraxasPrintJobManagerClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
            Query = "ossau",
        });
        printJobs.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListFilterAuthorityName()
    {
        await RunOnDb(async db =>
        {
            var printJob = await db.PrintJobs.AsTracking()
                .Include(x => x.DomainOfInfluence!.StepStates)
                .SingleAsync(x => x.Id == PrintJobMockData.BundFutureApprovedGemeindeArneggReadyGuid);

            printJob.DomainOfInfluence!.GenerateVotingCardsTriggered = MockedClock.GetDate();
            printJob.State = Data.Models.PrintJobState.Done;
            printJob.ProcessStartedOn = MockedClock.GetDate(0, 6);
            printJob.ProcessEndedOn = MockedClock.GetDate(0, 4);
            printJob.DoneOn = MockedClock.GetDate(0, 2);

            foreach (var stepState in printJob.DomainOfInfluence!.StepStates!.Where(s => s.Step <= Data.Models.Step.GenerateVotingCards))
            {
                stepState.Approved = true;
            }

            await db.SaveChangesAsync();
        });

        var printJobs = await AbraxasPrintJobManagerClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
            Query = "nEg",
        });
        printJobs.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListOnlyReadyForProcess()
    {
        var printJobs = await AbraxasPrintJobManagerClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
            State = PrintJobState.ReadyForProcess,
        });
        printJobs.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListElectionAdminAndContestManager()
    {
        var printJobs = await AbraxasElectionAdminClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.Summaries.Should().HaveCount(6);
    }

    [Fact]
    public async Task ListElectionAdminAndNotContestManagerShouldBeEmpty()
    {
        var printJobs = await GemeindeArneggElectionAdminClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.Summaries.Should().BeEmpty();
    }

    [Fact]
    public async Task ListPrintJobManager()
    {
        var printJobs = await AbraxasPrintJobManagerClient.ListSummariesAsync(new()
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.Summaries.Should().HaveCount(6);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.ListSummariesAsync(new() { ContestId = ContestMockData.BundFutureId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
