// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using AttachmentState = Voting.Stimmunterlagen.Proto.V1.Models.AttachmentState;
using PrintJobState = Voting.Stimmunterlagen.Proto.V1.Models.PrintJobState;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class SetAttachmentStateTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public SetAttachmentStateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSetStateAndCreateComment()
    {
        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest());

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == AttachmentMockData.BundFutureApprovedKantonStGallenGuid));

        foreach (var comment in attachment!.Comments!)
        {
            comment.Id = Guid.Empty;
            comment.Attachment = null;
        }

        attachment.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldUpdatePrintJobState()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;
        var attachmentId = AttachmentMockData.BundFutureApprovedGemeindeArneggId;

        await ModifyDbEntities<Data.Models.Attachment>(
            x => x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureApprovedGuid && x.DomainOfInfluenceId != doiId,
            x => x.State = Data.Models.AttachmentState.Delivered);

        await ModifyDbEntities<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == doiId,
            x => x.State = Data.Models.PrintJobState.SubmissionOngoing);

        await ModifyDbEntities<Data.Models.ContestDomainOfInfluence>(
            x => x.Id == doiId,
            x => x.GenerateVotingCardsTriggered = MockedClock.UtcNowDate);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x => x.Id = AttachmentMockData.BundFutureApprovedGemeindeArneggWithParentPbsId));
        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.SubmissionOngoing);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x => x.Id = attachmentId));
        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.ReadyForProcess);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x =>
        {
            x.Id = attachmentId;
            x.State = AttachmentState.Defined;
        }));
        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.SubmissionOngoing);
    }

    [Fact]
    public async Task ShouldNotUpdatePrintJobStateIfOnlySelfDefinedAttachmentsAreDelivered()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid;
        var attachmentId = AttachmentMockData.BundFutureApprovedGemeindeArneggId;

        await ModifyDbEntities<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == doiId,
            x => x.State = Data.Models.PrintJobState.SubmissionOngoing);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x => x.Id = attachmentId));

        // Arnegg has only 1 Attachment defined on the Arnegg Doi, but inherit others from its parents. Because this are not delivered, the state should not change.
        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.SubmissionOngoing);
    }

    [Fact]
    public async Task ShouldNotUpdatePrintJobStateWhenPrintJobProcessStarted()
    {
        var doiId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid;
        var attachmentId = AttachmentMockData.BundFutureApprovedGemeindeArneggId;

        await ModifyDbEntities<Data.Models.PrintJob>(
            x => x.DomainOfInfluenceId == doiId,
            x => x.State = Data.Models.PrintJobState.ProcessStarted);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x => x.Id = attachmentId));

        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.ProcessStarted);

        await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest(x =>
        {
            x.Id = attachmentId;
            x.State = AttachmentState.Defined;
        }));

        (await FindDbEntity<Data.Models.PrintJob>(x => x.DomainOfInfluenceId == doiId)).State.Should().HaveSameNameAs(PrintJobState.ProcessStarted);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.SetStateAsync(
                NewValidRequest(x => x.Id = AttachmentMockData.BundArchivedGemendeArneggId)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfExternalPrintingCenter()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
            x => x.ExternalPrintingCenter = true);
        await AssertStatus(async () => await AbraxasPrintJobManagerClient.SetStateAsync(NewValidRequest()), StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.SetStateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.ElectionAdmin;
    }

    private static SetAttachmentStateRequest NewValidRequest(Action<SetAttachmentStateRequest>? customizer = null)
    {
        var request = new SetAttachmentStateRequest
        {
            Id = AttachmentMockData.BundFutureApprovedKantonStGallenId,
            State = AttachmentState.Delivered,
            Comment = "comment",
        };

        customizer?.Invoke(request);
        return request;
    }
}
