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
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using Step = Voting.Stimmunterlagen.Data.Models.Step;

namespace Voting.Stimmunterlagen.IntegrationTest.ManualVotingCardGeneratorJobTests;

public class CreateEmptyManualVotingCardGeneratorJobTest : BaseWriteableDbGrpcTest<ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient>
{
    public CreateEmptyManualVotingCardGeneratorJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SetStepState(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, Step.GenerateVotingCards, true);

        await ModifyDbEntities<Data.Models.Contest>(
            c => c.Id == ContestMockData.BundFutureApprovedGuid,
            c => c.IsPoliticalAssembly = true);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp = await GemeindeArneggElectionAdminClient.CreateEmptyAsync(NewValidRequest());
        resp.Should().NotBeNull();
        resp.Pdf.ShouldBeAPdf();

        var job = await RunOnDb(db => db.ManualVotingCardGeneratorJobs
            .Where(x => x.Created == MockedClock.UtcNowDate
                        && x.Layout.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .Include(x => x.Voter)
            .Include(x => x.Layout)
            .SingleAsync());

        job.Id = Guid.Empty;
        job.LayoutId = Guid.Empty;
        job.Layout.Id = Guid.Empty;
        job.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfVotingCardsNotGenerated()
    {
        await SetStepState(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, Step.GenerateVotingCards, false);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateEmptyAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "GenerateVotingCards not found or has not the correct state");
    }

    [Fact]
    public async Task ShouldThrowIfForeignDomainOfInfluence()
    {
        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.CreateEmptyAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotPoliticalAssembly()
    {
        await ModifyDbEntities<Data.Models.Contest>(
            c => c.Id == ContestMockData.BundFutureApprovedGuid,
            c => c.IsPoliticalAssembly = false);

        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateEmptyAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "Empty voting cards are only supported in political assemblies");
    }

    protected override async Task AuthorizationTestCall(ManualVotingCardGeneratorJobsService.ManualVotingCardGeneratorJobsServiceClient service)
    {
        await service.CreateEmptyAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private CreateEmptyManualVotingCardGeneratorJobRequest NewValidRequest(Action<CreateEmptyManualVotingCardGeneratorJobRequest>? action = null)
    {
        var req = new CreateEmptyManualVotingCardGeneratorJobRequest()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
        action?.Invoke(req);
        return req;
    }
}
