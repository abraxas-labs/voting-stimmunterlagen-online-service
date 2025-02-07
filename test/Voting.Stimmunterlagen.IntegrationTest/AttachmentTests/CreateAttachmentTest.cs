// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class CreateAttachmentTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public CreateAttachmentTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldCreate()
    {
        await ModifyDbEntities<Data.Models.VoterList>(
            x => x.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid,
            x => x.NumberOfVoters = 10);

        var req = NewValidRequest();
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(req);

        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.PoliticalBusinessEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .SingleAsync(x => x.Id == id));

        attachment.TotalRequiredForVoterListsCount.Should().Be(5);

        var doiCount = attachment.DomainOfInfluenceAttachmentCounts!.Single();
        doiCount.DomainOfInfluenceId.Should().Be(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId);
        doiCount.RequiredForVoterListsCount.Should().Be(5);

        attachment.PoliticalBusinessEntries!
            .Select(x => x.PoliticalBusinessId)
            .OrderBy(x => x)
            .Should()
            .BeEquivalentTo(req.PoliticalBusinessIds.Select(Guid.Parse).OrderBy(x => x));

        attachment.Id = Guid.Empty;
        attachment.PoliticalBusinessEntries = null!;
        attachment.DomainOfInfluenceAttachmentCounts = null!;
        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldCreateInPoliticalAssembly()
    {
        await ModifyDbEntities<Data.Models.VoterList>(
            x => x.Id == VoterListMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid,
            x => x.NumberOfVoters = 10);

        var req = NewValidRequest(x =>
        {
            x.PoliticalBusinessIds.Clear();
            x.DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggId;
        });
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(req);

        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .SingleAsync(x => x.Id == id));

        attachment.TotalRequiredForVoterListsCount.Should().Be(3);

        var doiCount = attachment.DomainOfInfluenceAttachmentCounts!.Single();
        doiCount.DomainOfInfluenceId.Should().Be(DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggId);
        doiCount.RequiredForVoterListsCount.Should().Be(3);

        attachment.Id = Guid.Empty;
        attachment.DomainOfInfluenceAttachmentCounts = null!;
        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfNonZeroRequiredCountOnPoliticalParentType()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.CreateAsync(NewValidRequest(x =>
            {
                x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId;
                x.OrderedCount = 500;
                x.RequiredCount = 1;
                x.PoliticalBusinessIds.Clear();
                x.PoliticalBusinessIds.Add(VoteMockData.BundFutureApproved1Id);
            })),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Ch must have a ordered count greater than 0 and required count 0");
    }

    [Fact]
    public async Task ShouldThrowIfNonEqualCountOnNonPoliticalParentType()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest(x => x.OrderedCount = 1)),
            StatusCode.InvalidArgument,
            "Attachments on domain of influence of type Mu must have an equal ordered and required count and it has to be larger than 0");
    }

    [Fact]
    public async Task ShouldCreateIfDeliveryOnSameDayAsDeadline()
    {
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest(x => x.DeliveryPlannedOn = MockedClock.GetTimestampDate(15)));
        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments.SingleAsync(x => x.Id == id));
        attachment.DeliveryPlannedOn.Should().Be(MockedClock.GetDate(15).Date);
    }

    [Fact]
    public async Task ShouldThrowIfNotPbAttendee()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest(x =>
            {
                x.PoliticalBusinessIds.Clear();
                x.PoliticalBusinessIds.Add(VoteMockData.BundFutureApprovedStadtUzwil1Id);
            })),
            StatusCode.PermissionDenied,
            "only political businesses with permission entries allowed which are not on a domain of influence with external printing center");
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(
                NewValidRequest(x => x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId)),
            StatusCode.PermissionDenied,
            "no permissions on contest or domain of influence or the domain of influence cannot have attachments");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(
                NewValidRequest(x =>
                {
                    x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId;
                    x.PoliticalBusinessIds.Clear();
                    x.PoliticalBusinessIds.Add(VoteMockData.BundArchivedGemeindeArnegg1Id);
                })),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfDeliveryDateAfterDeadline()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(
                NewValidRequest(x => x.DeliveryPlannedOn = MockedClock.GetTimestampDate(16))),
            StatusCode.InvalidArgument,
            "delivery deadline");
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.CreateAsync(NewValidRequest()),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await AssertStatus(
            async () => await StadtUzwilElectionAdminClient.CreateAsync(NewValidRequest(x =>
            {
                x.DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilId;
                x.PoliticalBusinessIds.Clear();
                x.PoliticalBusinessIds.Add(VoteMockData.BundFutureApprovedStadtUzwil1Id);
            })),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldCreateIfParentPb()
    {
        var req = NewValidParentPbRequest();
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(req);

        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.PoliticalBusinessEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .SingleAsync(x => x.Id == id));

        attachment.TotalRequiredForVoterListsCount.Should().Be(9);

        var doiCount = attachment.DomainOfInfluenceAttachmentCounts!.Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        doiCount.RequiredForVoterListsCount.Should().Be(9);
        doiCount.RequiredCount.Should().Be(req.RequiredCount);

        attachment.PoliticalBusinessEntries!
            .Select(x => x.PoliticalBusinessId)
            .OrderBy(x => x)
            .Should()
            .BeEquivalentTo(req.PoliticalBusinessIds.Select(Guid.Parse).OrderBy(x => x));

        attachment.Id = Guid.Empty;
        attachment.PoliticalBusinessEntries = null!;
        attachment.DomainOfInfluenceAttachmentCounts = null!;
        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldCreateWithTotalNumberOfVotersAsRequiredForVoterListsCount()
    {
        await ModifyDbEntities<Data.Models.VoterList>(
            x => x.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid,
            x => x.NumberOfVoters = 10);

        var req = NewValidRequest(x => x.SendOnlyToHouseholder = false);
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(req);

        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.PoliticalBusinessEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .SingleAsync(x => x.Id == id));

        attachment.TotalRequiredForVoterListsCount.Should().Be(12);

        var doiCount = attachment.DomainOfInfluenceAttachmentCounts!.Single();
        doiCount.DomainOfInfluenceId.Should().Be(DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId);
        doiCount.RequiredForVoterListsCount.Should().Be(12);
    }

    [Fact]
    public async Task ShouldCreatePoliticalAssemblyWithTotalNumberOfVotersAsRequiredForVoterListsCount()
    {
        await ModifyDbEntities<Data.Models.VoterList>(
            x => x.Id == VoterListMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid,
            x => x.NumberOfVoters = 10);

        var req = NewValidRequest(x =>
        {
            x.PoliticalBusinessIds.Clear();
            x.DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggId;
            x.SendOnlyToHouseholder = false;
        });
        var response = await GemeindeArneggElectionAdminClient.CreateAsync(req);

        var id = Guid.Parse(response.Id);
        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .SingleAsync(x => x.Id == id));

        attachment.TotalRequiredForVoterListsCount.Should().Be(10);

        var doiCount = attachment.DomainOfInfluenceAttachmentCounts!.Single();
        doiCount.RequiredForVoterListsCount.Should().Be(10);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.CreateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static CreateAttachmentRequest NewValidRequest(Action<CreateAttachmentRequest>? customizer = null)
    {
        var request = new CreateAttachmentRequest
        {
            Name = "Publikation Gossau",
            Category = AttachmentCategory.BallotMu,
            Format = AttachmentFormat.A5,
            Color = "Blue",
            Supplier = "Lieferant",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(10),
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            PoliticalBusinessIds =
                {
                    VoteMockData.BundFutureApprovedGemeindeArnegg1Id,
                    ProportionalElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
                },
            OrderedCount = 2000,
            RequiredCount = 2000,
            SendOnlyToHouseholder = true,
        };
        customizer?.Invoke(request);
        return request;
    }

    private static CreateAttachmentRequest NewValidParentPbRequest(Action<CreateAttachmentRequest>? customizer = null)
    {
        var request = new CreateAttachmentRequest
        {
            Name = "Publikation Gossau",
            Category = AttachmentCategory.BallotMu,
            Format = AttachmentFormat.A5,
            Color = "Blue",
            Supplier = "Lieferant",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(10),
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            PoliticalBusinessIds =
                {
                    VoteMockData.BundFutureApproved1Id,
                },
            OrderedCount = 2000,
            RequiredCount = 2000,
        };
        customizer?.Invoke(request);
        return request;
    }
}
