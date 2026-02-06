// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using PrintJob = Voting.Stimmunterlagen.Data.Models.PrintJob;
using PrintJobState = Voting.Stimmunterlagen.Data.Models.PrintJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class ListPrintJobGenerateVotingCardsTriggeredTest : BaseWriteableDbGrpcTest<PrintJobService.PrintJobServiceClient>
{
    public ListPrintJobGenerateVotingCardsTriggeredTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.ContestId == ContestMockData.BundFutureApprovedGuid && x.BasisDomainOfInfluenceId != DomainOfInfluenceMockData.StadtGossauGuid,
            x => x.GenerateVotingCardsTriggered = MockedClock.GetDate());

        await ModifyDbEntities<PrintJob>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid,
            x => x.State = PrintJobState.ProcessEnded);
    }

    [Fact]
    public async Task ListForContest()
    {
        var printJobs = await AbraxasElectionAdminClient.ListGenerateVotingCardsTriggeredAsync(new ListPrintJobGenerateVotingCardsTriggeredRequest
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyForForNonContestManager()
    {
        var printJobs = await GemeindeArneggElectionAdminClient.ListGenerateVotingCardsTriggeredAsync(new ListPrintJobGenerateVotingCardsTriggeredRequest
        {
            ContestId = ContestMockData.BundFutureApprovedId,
        });
        printJobs.PrintJobs_.Should().HaveCount(0);
    }

    [Fact]
    public async Task ShouldReturnForPrintJobManager()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid,
            x => x.GenerateVotingCardsTriggered = DateTime.UtcNow);

        var printJobs = await AbraxasPrintJobManagerClient.ListGenerateVotingCardsTriggeredAsync(new ListPrintJobGenerateVotingCardsTriggeredRequest
        {
            ContestId = ContestMockData.SchulgemeindeAndwilArneggFutureId,
        });
        printJobs.PrintJobs_.Should().HaveCount(1);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.ListGenerateVotingCardsTriggeredAsync(new ListPrintJobGenerateVotingCardsTriggeredRequest { ContestId = ContestMockData.BundFutureId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
