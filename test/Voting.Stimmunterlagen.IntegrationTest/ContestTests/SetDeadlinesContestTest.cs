// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
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

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class SetDeadlinesContestTest : BaseWriteableDbGrpcTest<ContestService.ContestServiceClient>
{
    public SetDeadlinesContestTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSetDeadlines()
    {
        await AbraxasElectionAdminClient.SetDeadlinesAsync(NewRequest());

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(15).Date.NextUtcDate(true));
        contest.GenerateVotingCardsDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
        contest.ElectoralRegisterEVotingFrom.Should().Be(MockedClock.GetDate(9).Date.NextUtcDate(true));
    }

    [Fact]
    public async Task ShouldSetDeadlineIfAlreadySetInPast()
    {
        await ModifyDbEntities<Contest>(
            c => c.Id == ContestMockData.BundFutureGuid,
            c =>
            {
                c.PrintingCenterSignUpDeadline = MockedClock.GetDate(-1000).Date;
                c.PrintingCenterSignUpDeadline = MockedClock.GetDate(1000).Date;
                c.AttachmentDeliveryDeadline = MockedClock.GetDate(-995).Date;
            });

        await AbraxasElectionAdminClient.SetDeadlinesAsync(NewRequest());

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(15).Date.NextUtcDate(true));
        contest.ElectoralRegisterEVotingFrom.Should().Be(MockedClock.GetDate(9).Date.NextUtcDate(true));
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(NewRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfPrintingCenterSignUpDeadlineInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Contest deadlines must not be in the past");
    }

    [Fact]
    public Task ShouldThrowIfAttachmentDeliveryDeadlineInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Contest deadlines must not be in the past");
    }

    [Fact]
    public Task ShouldThrowIfGenerateVotingCardsDeadlineInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Contest deadlines must not be in the past");
    }

    [Fact]
    public Task ShouldThrowIfElectoralRegisterEVotingFromInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.ElectoralRegisterEVotingFromDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Electoral register e-voting from must not be in the past");
    }

    [Fact]
    public async Task ShouldThrowIfElectoralRegisterEVotingInNonEVotingContest()
    {
        await ModifyDbEntities<Contest>(
            x => x.Id == ContestMockData.BundFutureGuid,
            x => x.EVoting = false);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDeadlinesAsync(NewRequest()),
            StatusCode.InvalidArgument,
            "Cannot set electoral register e-voting date when contest supports no e-evoting");
    }

    [Fact]
    public async Task ShouldThrowIfNoElectoralRegisterEVotingInEVotingContest()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.ElectoralRegisterEVotingFromDate = null)),
            StatusCode.InvalidArgument,
            "Electoral register e-voting date is required for e-voting contest");
    }

    [Fact]
    public async Task ShouldThrowIfElectoralRegisterEVotingBeforeGenerateVotingCards()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundFutureId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(10),
                ElectoralRegisterEVotingFromDate = MockedClock.GetTimestampDate(11),
            }),
            StatusCode.InvalidArgument,
            "Electoral register e-voting date is required for e-voting contest and must take place before the generate voting cards deadline");
    }

    [Fact]
    public async Task ShouldThrowIfCommunalContest()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.BundGuid,
            x => x.Type = DomainOfInfluenceType.Mu);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDeadlinesAsync(NewRequest()),
            StatusCode.InvalidArgument,
            "Cannot set non-communal deadlines on communal contest");
    }

    [Fact]
    public async Task ShouldSetDeadlineIfSameDay()
    {
        await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
        {
            Id = ContestMockData.BundFutureId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(1),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(1),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(1),
            ElectoralRegisterEVotingFromDate = MockedClock.GetTimestampDate(1),
        });

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(1).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(1).Date.NextUtcDate(true));
        contest.GenerateVotingCardsDeadline.Should().Be(MockedClock.GetDate(1).Date.NextUtcDate(true));
        contest.ElectoralRegisterEVotingFrom.Should().Be(MockedClock.GetDate(0).Date.NextUtcDate(true));
    }

    [Fact]
    public Task ShouldThrowIfArchived()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(
                NewRequest(x => x.ElectoralRegisterEVotingFromDate = null)),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfPrintingCenterSignUpAfterGenerateVotingCardsDeadline()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundFutureId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(9),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
            }),
            StatusCode.InvalidArgument,
            "Printing center sign up deadline must take place earlier or at the same date than generate voting cards deadline");
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
    {
        await service.SetDeadlinesAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private SetContestDeadlinesRequest NewRequest(Action<SetContestDeadlinesRequest>? action = null)
    {
        var req = new SetContestDeadlinesRequest
        {
            Id = ContestMockData.BundFutureId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
            ElectoralRegisterEVotingFromDate = MockedClock.GetTimestampDate(10),
        };

        action?.Invoke(req);
        return req;
    }
}
