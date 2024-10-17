// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.StepTest.PoliticalBusinessApproval;

public class ApprovePoliticalBusinessesStepTest : BaseWriteableStepTest
{
    public ApprovePoliticalBusinessesStepTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.PoliticalBusinessesApproval,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.PoliticalBusinessesApproval,
            true);

        var politicalBusinesses = await RunOnDb(db => db.PoliticalBusinesses
            .Where(x =>
                x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureGuid
                && x.DomainOfInfluence!.SecureConnectId == MockDataSeeder.SecureConnectTenantIds.StadtGossau)
            .ToListAsync());
        politicalBusinesses.Should().NotBeEmpty();
        politicalBusinesses.All(x => x.Approved).Should().BeTrue();

        var printJobs = await RunOnDb(db => db.PrintJobs
            .Where(x =>
                x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureGuid
                && x.DomainOfInfluence!.SecureConnectId == MockDataSeeder.SecureConnectTenantIds.StadtGossau)
            .ToListAsync());
        printJobs.Should().NotBeEmpty();
        printJobs.All(x => x.State == Data.Models.PrintJobState.SubmissionOngoing).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotUpdatePrintJobStateIfAllAttachmentsOnceDelivered()
    {
        await ModifyDbEntities<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            x => x.State = Data.Models.PrintJobState.ReadyForProcess);

        await StadtGossauElectionAdminClient.ApproveAsync(new ApproveStepRequest
        {
            Step = Step.PoliticalBusinessesApproval,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauId,
        });
        await AssertStepApproved(
            DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
            Step.PoliticalBusinessesApproval,
            true);

        var printJobs = await RunOnDb(db => db.PrintJobs
            .Where(x =>
                x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureGuid
                && x.DomainOfInfluence!.SecureConnectId == MockDataSeeder.SecureConnectTenantIds.StadtGossau)
            .ToListAsync());
        printJobs.Should().NotBeEmpty();
        printJobs.All(x => x.State == Data.Models.PrintJobState.SubmissionOngoing).Should().BeFalse();
        printJobs.Any(x => x.State == Data.Models.PrintJobState.ReadyForProcess).Should().BeTrue();
    }
}
