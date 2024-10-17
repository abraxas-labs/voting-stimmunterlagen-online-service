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

public class AssignPoliticalBusinessVoterListTest : BaseWriteableDbGrpcTest<VoterListService.VoterListServiceClient>
{
    public AssignPoliticalBusinessVoterListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldAddPoliticalBusinessToVoterList()
    {
        await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(new AssignPoliticalBusinessVoterListRequest
        {
            Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId,
            PoliticalBusinessId = VoteMockData.BundFutureApproved2Id,
        });

        var politicalBusinessIds = await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid)
            .Select(x => x.PoliticalBusinessId)
            .ToListAsync());

        politicalBusinessIds.Should().HaveCount(5);
        politicalBusinessIds.Should().Contain(VoteMockData.BundFutureApproved2Guid);

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund1Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(3);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(3);
    }

    [Fact]
    public async Task ShouldThrowIfNotAttendee()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(
                NewValidRequest(x => x.PoliticalBusinessId = VoteMockData.SchulgemeindeAndwilArneggFuture1Id)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.AssignPoliticalBusinessAsync(
                NewValidRequest(x => x.Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfDuplicate()
    {
        await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(NewValidRequest());
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(
                new AssignPoliticalBusinessVoterListRequest
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
            async () => await GemeindeArneggElectionAdminClient.AssignPoliticalBusinessAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VoterListService.VoterListServiceClient service)
    {
        await service.AssignPoliticalBusinessAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static AssignPoliticalBusinessVoterListRequest NewValidRequest(Action<AssignPoliticalBusinessVoterListRequest>? customizer = null)
    {
        var request = new AssignPoliticalBusinessVoterListRequest
        {
            Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId,
            PoliticalBusinessId = ProportionalElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
        };

        customizer?.Invoke(request);
        return request;
    }
}
