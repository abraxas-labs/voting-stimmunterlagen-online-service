// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using PrintJob = Voting.Stimmunterlagen.Data.Models.PrintJob;
using PrintJobState = Voting.Stimmunterlagen.Data.Models.PrintJobState;
using StepState = Voting.Stimmunterlagen.Data.Models.StepState;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class UpdateContestPrintingCenterSignupDeadlineTest : BaseWriteableDbGrpcTest<ContestService.ContestServiceClient>
{
    public UpdateContestPrintingCenterSignupDeadlineTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.ContestId == ContestMockData.BundFutureApprovedGuid && x.BasisDomainOfInfluenceId != DomainOfInfluenceMockData.StadtGossauGuid,
            x => x.GenerateVotingCardsTriggered = MockedClock.GetDate());

        await ModifyDbEntities<StepState>(
            x => x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid,
            x => x.Approved = true);
    }

    [Fact]
    public async Task ShouldUpdateContestPrintingCenterSignupDeadline()
    {
        await ModifyDbEntities<PrintJob>(
            p => p.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid,
            p => p.State = PrintJobState.ReadyForProcess);

        await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(NewRequest());

        var contest = await RunOnDb(db => db.Contests
            .Include(x => x.ContestDomainOfInfluences!)
                .ThenInclude(x => x.StepStates)
            .Include(x => x.ContestDomainOfInfluences!)
                .ThenInclude(x => x.PrintJob)
            .SingleAsync(x => x.Id == ContestMockData.BundFutureApprovedGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(100).Date.NextUtcDate(true));
        contest.GenerateVotingCardsDeadline.Should().Be(MockedClock.GetDate(100).Date.NextUtcDate(true));

        var gemeindeArneggDoi = contest.ContestDomainOfInfluences!.Single(x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        gemeindeArneggDoi.GenerateVotingCardsTriggered.Should().BeNull();

        // only the passed dois should reset the generate voting cards triggered timestamp.
        contest.ContestDomainOfInfluences!.Any(doi => doi.GenerateVotingCardsTriggered != null).Should().BeTrue();
        gemeindeArneggDoi.PrintJob!.State.Should().Be(PrintJobState.SubmissionOngoing);
        var generateVotingCardsStepStates = contest.ContestDomainOfInfluences!
            .SelectMany(doi => doi.StepStates!)
            .Where(s => s.Step == Data.Models.Step.GenerateVotingCards)
            .ToList();

        generateVotingCardsStepStates
            .Where(s => s.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid
                        || s.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
            .All(s => s.Approved)
            .Should().BeFalse();

        // only the passed dois should revert.
        generateVotingCardsStepStates.Any(s => s.Approved && s.DomainOfInfluence!.PrintJob!.State == PrintJobState.ReadyForProcess)
            .Should().BeTrue();
    }

    [Fact]
    public async Task ShouldUpdateContestPrintingCenterSignupDeadlineIfSameDay()
    {
        await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(NewRequest(d => d.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(0)));

        var contest = await RunOnDb(db => db.Contests
            .Include(x => x.ContestDomainOfInfluences!)
                .ThenInclude(x => x.StepStates)
            .SingleAsync(x => x.Id == ContestMockData.BundFutureApprovedGuid));
        contest.PrintingCenterSignUpDeadline.Should().Be(MockedClock.GetDate(0).Date.NextUtcDate(true));
    }

    [Fact]
    public async Task ShouldThrowIfDateInPast()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(
                NewRequest(x => x.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(-1))),
            StatusCode.InvalidArgument,
            "Printing center sign up deadline must not be in the past");
    }

    [Fact]
    public async Task ShouldThrowIfGenerateVotingCardsNotTriggered()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.GenerateVotingCardsTriggered = null);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(
                NewRequest(x =>
                {
                    x.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(4999);
                    x.GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(4999);
                })),
            StatusCode.InvalidArgument,
            "Some passed domain of influences have no generated voting cards or their print job processing already started or are not in the same contest");
    }

    [Fact]
    public async Task ShouldThrowIfPrintJobProcessAlreadyStarted()
    {
        await ModifyDbEntities<PrintJob>(
            x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
            x => x.State = PrintJobState.ProcessStarted);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(
                NewRequest(x =>
                {
                    x.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(4999);
                    x.GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(4999);
                })),
            StatusCode.InvalidArgument,
            "Some passed domain of influences have no generated voting cards or their print job processing already started or are not in the same contest");
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(NewRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfArchived()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(
                NewRequest(x => x.Id = ContestMockData.BundArchivedId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPrintingCenterSignUpAfterGenerateVotingCardsDeadline()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdatePrintingCenterSignUpDeadlineAsync(
                NewRequest(x =>
                {
                    x.PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(5);
                    x.GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(4);
                })),
            StatusCode.InvalidArgument,
            "Printing center sign up deadline must take place earlier or at the same date than generate voting cards deadline");
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
    {
        await service.UpdatePrintingCenterSignUpDeadlineAsync(NewRequest());
    }

    private static UpdateContestPrintingCenterSignupDeadlineRequest NewRequest(Action<UpdateContestPrintingCenterSignupDeadlineRequest>? customizer = null)
    {
        var request = new UpdateContestPrintingCenterSignupDeadlineRequest
        {
            Id = ContestMockData.BundFutureApprovedId,
            PrintingCenterSignUpDeadlineDate = MockedClock.GetTimestampDate(100),
            GenerateVotingCardsDeadlineDate = MockedClock.GetTimestampDate(100),
            ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds =
                {
                    DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
                },
        };

        customizer?.Invoke(request);
        return request;
    }
}
