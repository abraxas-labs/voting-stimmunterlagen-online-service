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

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class SetPrintJobProcessEndedTest : SetPrintJobStateBaseTest
{
    public SetPrintJobProcessEndedTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await UpdateState(Data.Models.PrintJobState.ProcessStarted);

        var request = NewValidRequest();
        await AbraxasPrintJobManagerClient.SetProcessEndedAsync(request);

        var printJob = await FindDbEntity<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid);

        printJob.State.Should().Be(Data.Models.PrintJobState.ProcessEnded);
        printJob.ProcessEndedOn.Should().NotBeNull();
        printJob.VotingCardsPrintedAndPackedCount.Should().Be(request.VotingCardsPrintedAndPackedCount);
        printJob.VotingCardsShipmentWeight.Should().Be(request.VotingCardsShipmentWeight);
    }

    [Fact]
    public async Task ShouldThrowIfWrongState()
    {
        await UpdateState(Data.Models.PrintJobState.Done);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetProcessEndedAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetProcessEndedAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DefaultDomainOfInfluenceGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.SetProcessEndedAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.SetProcessEndedAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static SetPrintJobProcessEndedRequest NewValidRequest(Action<SetPrintJobProcessEndedRequest>? customizer = null)
    {
        var request = new SetPrintJobProcessEndedRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            VotingCardsPrintedAndPackedCount = 1500,
            VotingCardsShipmentWeight = 250.5,
        };

        customizer?.Invoke(request);
        return request;
    }
}
