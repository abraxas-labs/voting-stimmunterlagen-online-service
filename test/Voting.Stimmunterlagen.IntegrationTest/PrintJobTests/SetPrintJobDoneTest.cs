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

public class SetPrintJobDoneTest : SetPrintJobStateBaseTest
{
    public SetPrintJobDoneTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await UpdateState(Data.Models.PrintJobState.ProcessEnded);

        var request = NewValidRequest();
        await AbraxasPrintJobManagerClient.SetDoneAsync(request);

        var printJob = await FindDbEntity<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == DefaultDomainOfInfluenceGuid);

        printJob.State.Should().Be(Data.Models.PrintJobState.Done);
        printJob.DoneOn.Should().NotBeNull();
        printJob.DoneComment.Should().Be(request.Comment);
    }

    [Fact]
    public async Task ShouldThrowIfWrongState()
    {
        await UpdateState(Data.Models.PrintJobState.ProcessStarted);

        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetDoneAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetDoneAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DefaultDomainOfInfluenceGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.SetDoneAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.SetDoneAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static SetPrintJobDoneRequest NewValidRequest(Action<SetPrintJobDoneRequest>? customizer = null)
    {
        var request = new SetPrintJobDoneRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            Comment = "comment",
        };

        customizer?.Invoke(request);
        return request;
    }
}
