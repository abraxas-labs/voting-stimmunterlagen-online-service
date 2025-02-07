// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.ContestApproval;

public class RevertApproveContestStepTest : BaseWriteableStepTest
{
    public RevertApproveContestStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var contestBefore = await RunOnDb(db => db.Contests.SingleAsync(c => c.Id == ContestMockData.BundFutureApprovedGuid));
        contestBefore.Approved.Should().NotBeNull();

        await AbraxasElectionAdminClient.RevertAsync(new RevertStepRequest
        {
            Step = Step.ContestApproval,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
        });

        var contestAfter = await RunOnDb(db => db.Contests.SingleAsync(c => c.Id == ContestMockData.BundFutureApprovedGuid));
        contestAfter.Approved.Should().BeNull();
    }
}
