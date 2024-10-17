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

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class SetDomainOfInfluenceAttachmentRequiredCountTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public SetDomainOfInfluenceAttachmentRequiredCountTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSetCount()
    {
        await StadtGossauElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest());

        var count = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .Where(x => x.AttachmentId == AttachmentMockData.BundFutureApprovedKantonStGallenGuid
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
            .SingleAsync());

        count.RequiredCount.Should().Be(10);
        count.RequiredForVoterListsCount.Should().Be(3);
    }

    [Fact]
    public async Task ShouldSetCountWithExternalPrintingCenter()
    {
        await StadtUzwilElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(new()
        {
            Id = AttachmentMockData.BundFutureApprovedBund1Id,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilId,
            RequiredCount = 9,
        });

        var count = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .SingleAsync(x => x.AttachmentId == AttachmentMockData.BundFutureApprovedBund1Guid
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid));

        count.RequiredCount.Should().Be(9);
        count.RequiredForVoterListsCount.Should().Be(0);
    }

    [Fact]
    public async Task ShouldThrowIfOwnerDoiAndNonZeroRequiredCountOnPoliticalParentType()
    {
        await RunOnDb(async db =>
        {
            db.PoliticalBusinessPermissions.Add(new()
            {
                Role = Data.Models.PoliticalBusinessRole.Attendee,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
                PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid,
                SecureConnectId = "secure-connect-id",
            });

            var doi = await db.ContestDomainOfInfluences.AsTracking().SingleAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid);
            doi.ResponsibleForVotingCards = true;

            await db.SaveChangesAsync();
        });

        await AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest(x =>
            {
                x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenId;
                x.RequiredCount = 1;
            })),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Ct must have a ordered count greater than 0 and required count 0");
    }

    [Fact]
    public async Task ShouldThrowIfOwnerDoiAndNonEqualCountOnNonPoliticalParentType()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest(x =>
            {
                x.Id = AttachmentMockData.BundFutureApprovedGemeindeArneggId;
                x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId;
                x.RequiredCount = 1;
            })),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Mu must have an equal ordered and required count and it has to be larger than 0");
    }

    [Fact]
    public async Task ShouldThrowIfNotChildDoi()
    {
        await AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(
                NewValidRequest(x =>
                {
                    x.Id = AttachmentMockData.BundFutureApprovedStadtGossauDeliveredId;
                    x.DomainOfInfluenceId = DomainOfInfluenceMockData.KantonStGallenId;
                })),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfChildAttachment()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(
                NewValidRequest(x =>
                {
                    x.Id = AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsId;
                    x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId;
                })),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(
                new SetDomainOfInfluenceAttachmentRequiredCountRequest
                {
                    Id = AttachmentMockData.BundArchivedGemendeArneggId,
                    DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId,
                    RequiredCount = 10,
                }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfManagerAndNotAttendeeAndSetCount()
    {
        await AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(
                new SetDomainOfInfluenceAttachmentRequiredCountRequest
                {
                    Id = AttachmentMockData.BundFutureApprovedKantonStGallenId,
                    DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenId,
                    RequiredCount = 10,
                }),
            StatusCode.PermissionDenied,
            "cannot set attachment count when not responsible for voting cards");
    }

    [Fact]
    public async Task ShouldThrowIfExcludedByOwner()
    {
        var request = NewValidRequest();

        await RunOnDb(async db =>
        {
            var doiAttachmentCount = await db.DomainOfInfluenceAttachmentCounts.SingleAsync(x => x.DomainOfInfluenceId == Guid.Parse(request.DomainOfInfluenceId) && x.AttachmentId == Guid.Parse(request.Id));
            db.DomainOfInfluenceAttachmentCounts.Remove(doiAttachmentCount);
            await db.SaveChangesAsync();
        });

        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest()),
            StatusCode.NotFound,
            "DomainOfInfluenceAttachmentCount");
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.SetDomainOfInfluenceAttachmentRequiredCountAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static SetDomainOfInfluenceAttachmentRequiredCountRequest NewValidRequest(Action<SetDomainOfInfluenceAttachmentRequiredCountRequest>? customizer = null)
    {
        var request = new SetDomainOfInfluenceAttachmentRequiredCountRequest
        {
            Id = AttachmentMockData.BundFutureApprovedKantonStGallenId,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
            RequiredCount = 10,
        };

        customizer?.Invoke(request);
        return request;
    }
}
