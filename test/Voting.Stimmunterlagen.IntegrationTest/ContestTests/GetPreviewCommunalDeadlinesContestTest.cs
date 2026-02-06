// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class GetPreviewCommunalDeadlinesContestTest : BaseWriteableDbGrpcTest<ContestService.ContestServiceClient>
{
    public GetPreviewCommunalDeadlinesContestTest(TestApplicationFactory factory)
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
    public async Task ShouldWork()
    {
        var response = await AbraxasElectionAdminClient.GetPreviewCommunalDeadlinesAsync(new()
        {
            Id = ContestMockData.BundFutureId,
            DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
        });
        response.ShouldMatchSnapshot();
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await StadtGossauElectionAdminClient.GetPreviewCommunalDeadlinesAsync(new()
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
            async () => await AbraxasElectionAdminClient.GetPreviewCommunalDeadlinesAsync(new()
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
            async () => await AbraxasElectionAdminClient.GetPreviewCommunalDeadlinesAsync(new()
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
            async () => await AbraxasElectionAdminClient.GetPreviewCommunalDeadlinesAsync(new()
            {
                Id = ContestMockData.BundArchivedId,
                DeliveryToPostDeadlineDate = MockedClock.GetTimestampDate(31),
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestService.ContestServiceClient service)
    {
        await service.GetPreviewCommunalDeadlinesAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
