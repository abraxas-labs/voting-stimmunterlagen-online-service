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
        await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
        {
            Id = ContestMockData.BundFutureId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
        });

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(15).Date.NextUtcDate(true));
        contest.GenerateVotingCardsDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
    }

    [Fact]
    public async Task ShouldSetDeadlineIfAlreadySetInPast()
    {
        await ModifyDbEntities<Data.Models.Contest>(
            c => c.Id == ContestMockData.BundFutureGuid,
            c =>
            {
                c.PrintingCenterSignUpDeadline = MockedClock.GetDate(-1000).Date;
                c.PrintingCenterSignUpDeadline = MockedClock.GetDate(1000).Date;
                c.AttachmentDeliveryDeadline = MockedClock.GetDate(-995).Date;
            });

        await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
        {
            Id = ContestMockData.BundFutureId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
        });

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(10).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(15).Date.NextUtcDate(true));
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundFutureId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfPrintingCenterSignUpDeadlineInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundFutureId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(-1),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(1),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
            }),
            StatusCode.InvalidArgument,
            "Printing center sign up deadline must not be in the past");
    }

    [Fact]
    public Task ShouldThrowIfAttachmentDeliveryDeadlineInPast()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundFutureId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(-1),
            }),
            StatusCode.InvalidArgument,
            "Attachment delivery deadline must not be in the past");
    }

    [Fact]
    public async Task ShouldSetDeadlineIfSameDay()
    {
        await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
        {
            Id = ContestMockData.BundFutureId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(0),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(0),
            AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(0),
        });

        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(0).Date.NextUtcDate(true));
        contest.AttachmentDeliveryDeadline.Should().Be(MockedClock.GetDate(0).Date.NextUtcDate(true));
    }

    [Fact]
    public Task ShouldThrowIfArchived()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDeadlinesAsync(new SetContestDeadlinesRequest
            {
                Id = ContestMockData.BundArchivedId,
                PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(10),
                GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(10),
                AttachmentDeliveryDeadlineDate = MockedClock.GetTimestampDate(15),
            }),
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
}
