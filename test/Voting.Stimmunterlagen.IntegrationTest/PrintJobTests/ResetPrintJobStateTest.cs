// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using PrintJobState = Voting.Stimmunterlagen.Data.Models.PrintJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class ResetPrintJobStateTest : SetPrintJobStateBaseTest
{
    public ResetPrintJobStateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData(PrintJobState.ProcessStarted, PrintJobState.ReadyForProcess)]
    [InlineData(PrintJobState.ProcessEnded, PrintJobState.ProcessStarted)]
    [InlineData(PrintJobState.Done, PrintJobState.ProcessEnded)]
    public async Task ShouldWork(PrintJobState currentState, PrintJobState expectedResult)
    {
        await UpdateState(currentState);

        await AbraxasPrintJobManagerClient.ResetStateAsync(NewValidRequest());

        var printJob = await FindDbEntity<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid);

        printJob.State.Should().Be(expectedResult);

        switch (expectedResult)
        {
            case PrintJobState.ReadyForProcess:
                printJob.ProcessStartedOn.Should().BeNull();
                break;
            case PrintJobState.ProcessStarted:
                printJob.ProcessEndedOn.Should().BeNull();
                printJob.ProcessStartedOn.Should().NotBeNull();
                printJob.VotingCardsPrintedAndPackedCount.Should().Be(0);
                printJob.VotingCardsShipmentWeight.Should().Be(0);
                break;
            case PrintJobState.ProcessEnded:
                printJob.DoneOn.Should().BeNull();
                printJob.DoneComment.Should().BeEmpty();
                printJob.ProcessEndedOn.Should().NotBeNull();
                printJob.ProcessStartedOn.Should().NotBeNull();
                break;
        }
    }

    [Theory]
    [InlineData(PrintJobState.Unspecified)]
    [InlineData(PrintJobState.Empty)]
    [InlineData(PrintJobState.SubmissionOngoing)]
    public async Task ShouldThrowIfCurrentStateIsTooLow(PrintJobState currentState)
    {
        await UpdateState(currentState);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.ResetStateAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.ResetStateAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DefaultDomainOfInfluenceGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.ResetStateAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.ResetStateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static ResetPrintJobStateRequest NewValidRequest(Action<ResetPrintJobStateRequest>? customizer = null)
    {
        var request = new ResetPrintJobStateRequest
        {
            DomainOfInfluenceId = DefaultDomainOfInfluenceId,
        };

        customizer?.Invoke(request);
        return request;
    }
}
