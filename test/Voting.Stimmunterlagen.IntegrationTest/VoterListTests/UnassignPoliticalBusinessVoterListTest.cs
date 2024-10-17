// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListTests;

public class UnassignPoliticalBusinessVoterListTest : BaseWriteableDbGrpcTest<VoterListService.VoterListServiceClient>
{
    public UnassignPoliticalBusinessVoterListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldRemovePoliticalBusinessFromVoterList()
    {
        await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(new UnassignPoliticalBusinessVoterListRequest
        {
            Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId,
            PoliticalBusinessId = VoteMockData.BundFutureApproved1Id,
        });

        var politicalBusinessCount = await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid)
            .CountAsync());

        politicalBusinessCount.Should().Be(3);

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund2Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(10);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(6);
    }

    [Fact]
    public async Task ShouldThrowIfNotAttendee()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UnassignPoliticalBusinessAsync(
                NewValidRequest(x => x.PoliticalBusinessId = VoteMockData.SchulgemeindeAndwilArneggFuture1Id)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UnassignPoliticalBusinessAsync(
                NewValidRequest(x => x.Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(
                new UnassignPoliticalBusinessVoterListRequest
                {
                    Id = VoterListMockData.BundArchivedGemeindeArneggSwissId,
                    PoliticalBusinessId = VoteMockData.BundArchivedGemeindeArnegg1Id,
                }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UnassignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VoterListService.VoterListServiceClient service)
    {
        await service.UnassignPoliticalBusinessAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UnassignPoliticalBusinessVoterListRequest NewValidRequest(Action<UnassignPoliticalBusinessVoterListRequest>? customizer = null)
    {
        var request = new UnassignPoliticalBusinessVoterListRequest
        {
            Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId,
            PoliticalBusinessId = VoteMockData.BundFutureApprovedGemeindeArnegg1Id,
        };

        customizer?.Invoke(request);
        return request;
    }
}
