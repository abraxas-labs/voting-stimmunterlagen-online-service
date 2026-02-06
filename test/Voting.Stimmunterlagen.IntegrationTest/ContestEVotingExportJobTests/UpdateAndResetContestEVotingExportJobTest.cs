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
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using ContestEVotingExportJob = Voting.Stimmunterlagen.Data.Models.ContestEVotingExportJob;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using ExportJobState = Voting.Stimmunterlagen.Data.Models.ExportJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestEVotingExportJobTests;

public class UpdateAndResetContestEVotingExportJobTest : BaseWriteableDbGrpcTest<
    ContestEVotingExportJobService.ContestEVotingExportJobServiceClient>
{
    private const string DefaultContestId = ContestMockData.BundFutureApprovedId;
    private static readonly Guid DefaultContestGuid = ContestMockData.BundFutureApprovedGuid;

    public UpdateAndResetContestEVotingExportJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<StepState>(
            x => x.DomainOfInfluence!.ContestId == DefaultContestGuid && x.Step == Step.EVoting,
            x => x.Approved = true);
    }

    [Fact]
    public async Task ShouldWorkForContestManager()
    {
        await SetState(ExportJobState.Failed);

        await AbraxasElectionAdminClient.UpdateAndResetJobAsync(NewRequest(x => x.Ech0045Version = Proto.V1.Models.Ech0045Version._6));
        var job = await FindDbEntity<ContestEVotingExportJob>(x => x.ContestId == DefaultContestGuid);
        job.Id = Guid.Empty;
        job.State.Should().Be(ExportJobState.Pending);
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
            async () => await AbraxasElectionAdminClient.UpdateAndResetJobAsync(NewRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowForNonContestManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateAndResetJobAsync(NewRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestEVotingExportJobService.ContestEVotingExportJobServiceClient service)
    {
        await service.UpdateAndResetJobAsync(new());
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

    private UpdateAndResetContestEVotingExportJobRequest NewRequest(Action<UpdateAndResetContestEVotingExportJobRequest>? customizer = null)
    {
        var req = new UpdateAndResetContestEVotingExportJobRequest
        {
            ContestId = DefaultContestId,
            Ech0045Version = Proto.V1.Models.Ech0045Version._4,
        };

        customizer?.Invoke(req);
        return req;
    }
}
