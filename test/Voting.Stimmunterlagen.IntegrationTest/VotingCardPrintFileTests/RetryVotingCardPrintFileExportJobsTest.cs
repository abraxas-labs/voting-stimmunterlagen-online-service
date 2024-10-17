// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using ExportJobState = Voting.Stimmunterlagen.Data.Models.ExportJobState;
using VotingCardPrintFileExportJob = Voting.Stimmunterlagen.Data.Models.VotingCardPrintFileExportJob;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardPrintFileTests;

public class RetryVotingCardPrintFileExportJobsTest : BaseWriteableDbGrpcTest<
    VotingCardPrintFileExportJobService.VotingCardPrintFileExportJobServiceClient>
{
    public RetryVotingCardPrintFileExportJobsTest(TestApplicationFactory factory)
        : base(factory)
    {
        GetService<VotingCardPrintFileExportThrottlerMock>().ShouldBlock = true;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<VotingCardPrintFileExportJob>(
            x => x.Id == VotingCardPrintFileExportJobMockData.BundFutureApprovedGemeindeArneggJob1Guid,
            x => x.State = ExportJobState.Failed);
        await ModifyDbEntities<VotingCardPrintFileExportJob>(
            x => x.Id == VotingCardPrintFileExportJobMockData.BundFutureApprovedGemeindeArneggJob2Guid,
            x => x.State = ExportJobState.Completed);
    }

    [Fact]
    public async Task ShouldSetStatesReady()
    {
        await AbraxasPrintJobManagerClient.RetryAsync(NewValidRequest());

        var jobs = await FindDbEntities<VotingCardPrintFileExportJob>(x =>
            x.VotingCardGeneratorJob!.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        jobs.Should().HaveCount(3);
        jobs.Count(x => x.State == ExportJobState.ReadyToRun).Should().Be(2);
        jobs.Count(x => x.State == ExportJobState.Completed).Should().Be(1);
        GetService<VotingCardPrintFileExportThrottlerMock>().BlockedCount.Should().Be(2);
    }

    [Fact]
    public async Task ShouldNotUpdateStateIfNotInTestingPhase()
    {
        await SetContestState(ContestMockData.BundFutureApprovedGuid, ContestState.Active);

        await AbraxasPrintJobManagerClient.RetryAsync(NewValidRequest());

        var jobs = await FindDbEntities<VotingCardPrintFileExportJob>(x =>
            x.VotingCardGeneratorJob!.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        jobs.Should().HaveCount(3);
        jobs.Count(x => x.State == ExportJobState.ReadyToRun).Should().Be(1);
    }

    [Fact]
    public async Task ShouldNotUpdateStateIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.ExternalPrintingCenter = true);
        await AbraxasPrintJobManagerClient.RetryAsync(NewValidRequest());
        var jobs = await FindDbEntities<VotingCardPrintFileExportJob>(x =>
            x.VotingCardGeneratorJob!.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        jobs.Should().HaveCount(3);
        jobs.Count(x => x.State == ExportJobState.ReadyToRun).Should().Be(1);
    }

    protected override async Task AuthorizationTestCall(VotingCardPrintFileExportJobService.VotingCardPrintFileExportJobServiceClient service)
    {
        await service.RetryAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private RetryVotingCardPrintFileExportJobsRequest NewValidRequest()
    {
        return new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
    }
}
