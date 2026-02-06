// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListTests;

public class UpdateVoterListsTest : BaseWriteableDbGrpcTest<VoterListService.VoterListServiceClient>
{
    public UpdateVoterListsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWorkWithManualEchUpload()
    {
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedBund1Guid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(0);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(0);

        (await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid)
            .CountAsync())).Should().Be(4);

        await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromManualEchUpload());

        (await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid)
            .CountAsync())).Should().Be(1);

        attachment = await RunOnDb(db => db.Attachments
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
    public async Task ShouldWorkWithElectoralRegister()
    {
        (await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid)
            .CountAsync())).Should().Be(3);

        await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromElectoralRegister());

        (await RunOnDb(db => db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid)
            .CountAsync())).Should().Be(2);
    }

    [Fact]
    public async Task ShouldUpdateSendCountOfVotingCardsToDoiReturnAddressOnManualEchUpload()
    {
        await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromManualEchUpload(x => x.VoterLists[0].SendVotingCardsToDomainOfInfluenceReturnAddress = true));
        var afterSendToDoiReturnAddress = await RunOnDb(db => db.VoterLists
            .Include(x => x.Voters)
            .SingleAsync(x => x.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid));
        afterSendToDoiReturnAddress.SendVotingCardsToDomainOfInfluenceReturnAddress.Should().BeTrue();
        afterSendToDoiReturnAddress.CountOfVotingCardsForDomainOfInfluenceReturnAddress.Should().Be(3);
        afterSendToDoiReturnAddress.Voters!.Any().Should().BeTrue();
        afterSendToDoiReturnAddress.Voters!.All(v => v.SendVotingCardsToDomainOfInfluenceReturnAddress).Should().BeTrue();

        await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromManualEchUpload(x => x.VoterLists[0].SendVotingCardsToDomainOfInfluenceReturnAddress = false));
        var afterNotSendToDoiReturnAddress = await RunOnDb(db => db.VoterLists
            .Include(x => x.Voters)
            .SingleAsync(x => x.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid));
        afterNotSendToDoiReturnAddress.SendVotingCardsToDomainOfInfluenceReturnAddress.Should().BeFalse();
        afterNotSendToDoiReturnAddress.CountOfVotingCardsForDomainOfInfluenceReturnAddress.Should().Be(0);
        afterNotSendToDoiReturnAddress.Voters!.Any().Should().BeTrue();
        afterNotSendToDoiReturnAddress.Voters!.All(v => v.SendVotingCardsToDomainOfInfluenceReturnAddress).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromElectoralRegister());
        doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().Be(MockedClock.GetDate());
    }

    [Fact]
    public Task ShouldThrowIfContestLocked()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(
                new UpdateVoterListsRequest()
                {
                    VoterLists =
                    {
                        new UpdateVoterListRequest
                        {
                            Id = VoterListMockData.BundArchivedGemeindeArneggSwissId,
                            PoliticalBusinessIds = { VoteMockData.BundArchivedGemeindeArnegg1Id },
                        },
                    },
                }),
            StatusCode.InvalidArgument,
            "A provided voter list was not found");
    }

    [Fact]
    public Task ShouldThrowIfEVotingListWithSendToReturnDoiAddress()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(
                new UpdateVoterListsRequest
                {
                    VoterLists =
                    {
                        new UpdateVoterListRequest
                        {
                            Id = VoterListMockData.BundFutureApprovedGemeindeArneggEVoterId,
                            PoliticalBusinessIds = { VoteMockData.BundFutureApprovedGemeindeArnegg1Id },
                            SendVotingCardsToDomainOfInfluenceReturnAddress = true,
                        },
                    },
                }),
            StatusCode.InvalidArgument,
            "Cannot set SendVotingCardsToDomainOfInfluenceReturnAddress on a e-voting voter list");
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(NewValidRequestFromManualEchUpload()),
            StatusCode.InvalidArgument,
            "A provided voter list was not found");
    }

    [Fact]
    public Task ShouldThrowIfNotAttendee()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(
                NewValidRequestFromManualEchUpload(x => x.VoterLists[0].PoliticalBusinessIds.Add(VoteMockData.SchulgemeindeAndwilArneggFuture1Id))),
            StatusCode.PermissionDenied,
            "Invalid political business found");
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateListsAsync(
                NewValidRequestFromManualEchUpload()),
            StatusCode.InvalidArgument,
            "A provided voter list was not found");
    }

    [Fact]
    public Task ShouldThrowWithVoterListFromDifferentImport()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(
                NewValidRequestFromManualEchUpload(x => x.VoterLists.Add(new UpdateVoterListRequest { Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterId }))),
            StatusCode.InvalidArgument,
            "You can only update voter lists of the same import at once");
    }

    [Fact]
    public Task ShouldThrowWithAutoSendVotingCardsToDoiReturnAddressSplitAndNonNullSendVotingCardsToReturnDoiAddress()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateListsAsync(
                NewValidRequestFromElectoralRegister(x => x.VoterLists[0].SendVotingCardsToDomainOfInfluenceReturnAddress = false)),
            StatusCode.InvalidArgument,
            "Cannot set SendVotingCardsToDomainOfInfluenceReturnAddress on electoral register voter lists");
    }

    protected override async Task AuthorizationTestCall(VoterListService.VoterListServiceClient service)
    {
        await service.UpdateListsAsync(NewValidRequestFromManualEchUpload());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UpdateVoterListsRequest NewValidRequestFromManualEchUpload(Action<UpdateVoterListsRequest>? customizer = null)
    {
        var request = new UpdateVoterListsRequest()
        {
            VoterLists =
            {
                new UpdateVoterListRequest
                {
                    Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissId,
                    PoliticalBusinessIds = { VoteMockData.BundFutureApproved2Id },
                },
            },
        };

        customizer?.Invoke(request);
        return request;
    }

    private static UpdateVoterListsRequest NewValidRequestFromElectoralRegister(Action<UpdateVoterListsRequest>? customizer = null)
    {
        var request = new UpdateVoterListsRequest()
        {
            VoterLists =
            {
                new UpdateVoterListRequest
                {
                    Id = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterId,
                    PoliticalBusinessIds = { VoteMockData.BundFutureApproved1Id, VoteMockData.BundFutureApprovedKantonStGallen1Id },
                },
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
