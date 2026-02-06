// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class SetCommunalDeadlinesContestTest : BaseWriteableDbGrpcTest<ContestService.ContestServiceClient>
{
    public SetCommunalDeadlinesContestTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.BundGuid,
            x => x.Type = DomainOfInfluenceType.Mu);

        await ModifyDbEntities<Contest>(
            x => x.Id == ContestMockData.BundFutureGuid,
            x => x.Date = MockedClock.GetDate(32));
    }

    [Fact]
    public async Task ShouldSetDeadlines()
    {
        var response = await AbraxasElectionAdminClient.SetCommunalDeadlinesAsync(new()
        {
            Id = ContestMockData.BundFutureId,
            DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
        });
        response.ShouldMatchSnapshot();

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.DeliveryToPostDeadline.Should().Be(MockedClock.GetDate(31).Date.NextUtcDate(true));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(0).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(9).Date.NextUtcDate(true));
        contest.GenerateVotingCardsDeadline.Should().Be(MockedClock.GetDate(7).Date.NextUtcDate(true));
        contest.ElectoralRegisterEVotingFrom.Should().Be(null);
    }

    [Fact]
    public async Task ShouldSetDeadlineIfAlreadySetInPast()
    {
        await ModifyDbEntities<Contest>(
            c => c.Id == ContestMockData.BundFutureGuid,
            c =>
            {
                c.DeliveryToPostDeadline = MockedClock.GetDate(-1000).Date;
            });

        await AbraxasElectionAdminClient.SetCommunalDeadlinesAsync(new()
        {
            Id = ContestMockData.BundFutureId,
            DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
        });

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.DeliveryToPostDeadline.Should().Be(MockedClock.GetDate(31).Date.NextUtcDate(true));
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetCommunalDeadlinesAsync(new()
            {
                Id = ContestMockData.BundFutureId,
                DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfDeliveryToPostDeadlineInPast()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetCommunalDeadlinesAsync(new()
            {
                Id = ContestMockData.BundFutureId,
                DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(-1),
            }),
            StatusCode.InvalidArgument,
            "Contest deadlines must not be in the past");
    }

    [Fact]
    public async Task ShouldThrowIfNonCommunalContest()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.BundGuid,
            x => x.Type = DomainOfInfluenceType.Ct);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetCommunalDeadlinesAsync(new()
            {
                Id = ContestMockData.BundFutureId,
                DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
            }),
            StatusCode.InvalidArgument,
            "Cannot calculate communal deadlines on non-communal contest");
    }

    [Fact]
    public Task ShouldThrowIfArchived()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetCommunalDeadlinesAsync(new()
            {
                Id = ContestMockData.BundArchivedId,
                DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
    {
        await service.SetCommunalDeadlinesAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
