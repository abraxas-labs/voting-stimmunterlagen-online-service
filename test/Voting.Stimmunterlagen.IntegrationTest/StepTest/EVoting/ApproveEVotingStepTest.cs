// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Xunit;
using ContestEVotingExportJob = Voting.Stimmunterlagen.Data.Models.ContestEVotingExportJob;
using ExportJobState = Voting.Stimmunterlagen.Data.Models.ExportJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.EVoting;

public class ApproveEVotingStepTest : BaseWriteableStepTest
{
    private static readonly Guid _contestGuid = ContestMockData.BundFutureApprovedGuid;

    public ApproveEVotingStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await ModifyDbEntities<ContestEVotingExportJob>(
            x => x.ContestId == _contestGuid,
            x => x.State = ExportJobState.Pending);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId;
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid;

        await ModifyDbEntities<ContestEVotingExportJob>(
            x => x.ContestId == _contestGuid,
            x => x.State = ExportJobState.Pending);

        GetService<ContestEVotingExportThrottlerMock>().ShouldBlock = true;
        await SetStepApproved(doiGuid, Step.Attachments, true);
        await AbraxasElectionAdminClient.ApproveAsync(new()
        {
            DomainOfInfluenceId = doiId,
            Step = Step.EVoting,
        });

        await AssertStepApproved(
            doiGuid,
            Step.EVoting,
            true);

        var exportJob = await FindDbEntity<ContestEVotingExportJob>(x => x.ContestId == _contestGuid);
        exportJob.Should().NotBeNull();
        exportJob.State.Should().Be(ExportJobState.ReadyToRun);
        exportJob.FileName.Should().Be("eCH-0045_v4_0_SG_20200112_Contest 001 (BundFuture, approved) de_EVoting.zip");
    }
}
