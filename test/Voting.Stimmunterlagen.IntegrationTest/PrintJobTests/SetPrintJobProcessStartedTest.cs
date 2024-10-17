// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class SetPrintJobProcessStartedTest : SetPrintJobStateBaseTest
{
    public SetPrintJobProcessStartedTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await ModifyDbEntities<Data.Models.VotingCardGeneratorJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid,
            x =>
            {
                x.Completed = MockedClock.GetDate();
                x.State = Data.Models.VotingCardGeneratorJobState.Completed;
            });

        await ModifyDbEntities<Data.Models.ContestDomainOfInfluence>(
            x => x.Id == DefaultDomainOfInfluenceGuid,
            x => x.GenerateVotingCardsTriggered = MockedClock.GetDate());

        await UpdateState(Data.Models.PrintJobState.ReadyForProcess);
    }

    [Fact]
    public async Task ShouldWork()
    {
        await RunOnDb(async db =>
        {
            db.VotingCardPrintFileExportJobs.RemoveRange(await db.VotingCardPrintFileExportJobs.ToListAsync());
            await db.SaveChangesAsync();
        });

        var printFileExportJobs = await RunOnDb(db => db.VotingCardPrintFileExportJobs.Include(x => x.VotingCardGeneratorJob).ToListAsync());
        printFileExportJobs.Should().HaveCount(0);

        GetService<VotingCardPrintFileExportThrottlerMock>().ShouldBlock = true;
        await AbraxasPrintJobManagerClient.SetProcessStartedAsync(NewValidRequest());

        var printJob = await FindDbEntity<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid);

        printJob.State.Should().Be(Data.Models.PrintJobState.ProcessStarted);
        printJob.ProcessStartedOn.Should().NotBeNull();

        printFileExportJobs = await RunOnDb(db => db.VotingCardPrintFileExportJobs.Include(x => x.VotingCardGeneratorJob).ToListAsync());
        printFileExportJobs.Should().HaveCount(3);
        printFileExportJobs.All(x => x.State == Data.Models.ExportJobState.ReadyToRun).Should().BeTrue();

        var swissPrintFileExportJob = printFileExportJobs.Single(x => x.VotingCardGeneratorJobId == VotingCardGeneratorJobMockData.BundFutureApprovedGemeindeArneggJob1Guid);
        swissPrintFileExportJob.FileName.Should().Be("de.csv");
    }

    [Fact]
    public async Task ShouldThrowIfWrongState()
    {
        await UpdateState(Data.Models.PrintJobState.Empty);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetProcessStartedAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetProcessStartedAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfAnyVotingCardGeneratorJobIsNotCompleted()
    {
        await ModifyDbEntities<Data.Models.VotingCardGeneratorJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid,
            x =>
            {
                x.Completed = null;
                x.State = Data.Models.VotingCardGeneratorJobState.Running;
            });

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetProcessStartedAsync(
                NewValidRequest()),
            StatusCode.InvalidArgument,
            "job");
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DefaultDomainOfInfluenceGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.SetProcessStartedAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.SetProcessStartedAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static SetPrintJobProcessStartedRequest NewValidRequest(Action<SetPrintJobProcessStartedRequest>? customizer = null)
    {
        var request = new SetPrintJobProcessStartedRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };

        customizer?.Invoke(request);
        return request;
    }
}
