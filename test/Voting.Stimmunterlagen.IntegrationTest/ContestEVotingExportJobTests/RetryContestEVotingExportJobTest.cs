// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using ContestEVotingExportJob = Voting.Stimmunterlagen.Data.Models.ContestEVotingExportJob;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using ExportJobState = Voting.Stimmunterlagen.Data.Models.ExportJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestEVotingExportJobTests;

public class RetryContestEVotingExportJobTest : BaseWriteableDbGrpcTest<
    ContestEVotingExportJobService.ContestEVotingExportJobServiceClient>
{
    private const string DefaultContestId = ContestMockData.BundFutureApprovedId;
    private static readonly Guid DefaultContestGuid = ContestMockData.BundFutureApprovedGuid;

    public RetryContestEVotingExportJobTest(TestApplicationFactory factory)
        : base(factory)
    {
        GetService<ContestEVotingExportThrottlerMock>().ShouldBlock = true;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<StepState>(
            x => x.DomainOfInfluence!.ContestId == DefaultContestGuid && x.Step == Step.EVoting,
            x => x.Approved = true);
    }

    [Fact]
    public async Task ShouldSetStateReadyForContestManager()
    {
        await SetState(ExportJobState.Failed);

        await AbraxasElectionAdminClient.RetryJobAsync(new()
        {
            ContestId = DefaultContestId,
        });

        GetService<ContestEVotingExportThrottlerMock>().BlockedCount.Should().Be(1);
        var job = await FindDbEntity<ContestEVotingExportJob>(x => x.ContestId == DefaultContestGuid);
        job.Id = Guid.Empty;
        job.State.Should().Be(ExportJobState.ReadyToRun);
        job.Runner.Should().BeEmpty();
        job.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfNotInTestingPhase()
    {
        await SetState(ExportJobState.Completed);

        await ModifyDbEntities<Contest>(
            x => x.Id == DefaultContestGuid,
            x => x.State = ContestState.Active);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.RetryJobAsync(new()
            {
                ContestId = DefaultContestId,
            }),
            StatusCode.NotFound);

        var job = await FindDbEntity<ContestEVotingExportJob>(x => x.ContestId == DefaultContestGuid);
        job.State.Should().Be(ExportJobState.Completed);
    }

    [Fact]
    public async Task ShouldThrowForNonContestManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.RetryJobAsync(new()
            {
                ContestId = DefaultContestId,
            }),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowForContestManagerIfEVotingStepNotApproved()
    {
        await ModifyDbEntities<StepState>(
            x => x.DomainOfInfluence!.ContestId == DefaultContestGuid && x.Step == Step.EVoting,
            x => x.Approved = false);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.RetryJobAsync(new()
            {
                ContestId = DefaultContestId,
            }),
            StatusCode.PermissionDenied,
            "Cannot retry a job if the e-voting step is not approved yet");
    }

    [Theory]
    [InlineData(ExportJobState.ReadyToRun)]
    [InlineData(ExportJobState.Unspecified)]
    [InlineData(ExportJobState.Running)]
    public async Task ShouldThrowForInvalidJobState(ExportJobState state)
    {
        await SetState(state);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.RetryJobAsync(new()
            {
                ContestId = DefaultContestId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestEVotingExportJobService.ContestEVotingExportJobServiceClient service)
    {
        await service.RetryJobAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private Task SetState(ExportJobState state)
    {
        return ModifyDbEntities<ContestEVotingExportJob>(
            x => x.ContestId == DefaultContestGuid,
            x => x.State = state);
    }
}
