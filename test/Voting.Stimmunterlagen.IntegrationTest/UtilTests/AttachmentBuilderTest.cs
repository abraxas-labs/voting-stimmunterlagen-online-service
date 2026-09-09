// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.UtilTests;

public class AttachmentBuilderTest : BaseWriteableDbTest
{
    private AttachmentBuilder _attachmentBuilder;

    public AttachmentBuilderTest(TestApplicationFactory factory)
        : base(factory)
    {
        _attachmentBuilder = GetService<AttachmentBuilder>();
    }

    [Fact]
    public async Task CleanUpShouldIgnorePoliticalAssemblyWithNoPoliticalBusinessEntry()
    {
        await _attachmentBuilder.CleanUp(new[] { ContestMockData.PoliticalAssemblyBundFutureApprovedGuid });
        (await AttachmentExists(AttachmentMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid)).Should().BeTrue();
    }

    [Fact]
    public async Task CleanUpShouldDeleteIfNoPoliticalBusinessEntryExists()
    {
        var attachmentId = AttachmentMockData.BundFutureApprovedGemeindeArneggGuid;

        await _attachmentBuilder.CleanUp(new[] { ContestMockData.BundFutureApprovedGuid });
        (await AttachmentExists(attachmentId)).Should().BeTrue();

        await RunOnDb(async db =>
        {
            var entries = await db.PoliticalBusinessAttachmentEntries
                .Where(x => x.AttachmentId == attachmentId)
                .ToListAsync();

            db.PoliticalBusinessAttachmentEntries.RemoveRange(entries);
            await db.SaveChangesAsync();
        });

        await _attachmentBuilder.CleanUp(new[] { ContestMockData.BundFutureApprovedGuid });
        (await AttachmentExists(attachmentId)).Should().BeFalse();
    }

    [Fact]
    public async Task CleanUpShouldDeleteIfNoAttachmentStepExists()
    {
        await RunOnDb(async db =>
        {
            var stepState = await db.StepStates
                .SingleAsync(s => s.DomainOfInfluenceId == DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid && s.Step == Step.Attachments);

            db.StepStates.Remove(stepState);
            await db.SaveChangesAsync();
        });

        await _attachmentBuilder.CleanUp(new[] { ContestMockData.PoliticalAssemblyBundFutureApprovedGuid });
        (await AttachmentExists(AttachmentMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid)).Should().BeFalse();
    }

    [Fact]
    public async Task SyncForBasisDomainOfInfluenceShouldAddAndRemoveCrossTenantHostAttachmentCount()
    {
        var hostAttachmentId = AttachmentMockData.BundFutureApprovedStadtGossauDeliveredGuid;
        var attendeeContestDoiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .CountAsync(x => x.AttachmentId == hostAttachmentId && x.DomainOfInfluenceId == attendeeContestDoiId)))
            .Should().Be(0);

        // Add GemeindeArnegg's counting circle to Doi StadtGossau.
        // Doi StadtGossau becomes a host of "main voting card" Doi GemeindeArnegg, so GemeindeArnegg attends now to the StadtGossau attachments.
        await UpdateCountingCircles(DomainOfInfluenceMockData.StadtGossauGuid, new[] { CountingCircleMockData.StadtGossauGuid, CountingCircleMockData.GemeindeArneggGuid });

        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.StadtGossauGuid);
        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .CountAsync(x => x.AttachmentId == hostAttachmentId && x.DomainOfInfluenceId == attendeeContestDoiId)))
            .Should().Be(1);

        await UpdateCountingCircles(DomainOfInfluenceMockData.StadtGossauGuid, new[] { CountingCircleMockData.StadtGossauGuid });
        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.StadtGossauGuid);

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .CountAsync(x => x.AttachmentId == hostAttachmentId && x.DomainOfInfluenceId == attendeeContestDoiId)))
            .Should().Be(0);
    }

    [Fact]
    public async Task SyncForBasisDomainOfInfluenceShouldAddAndRemoveHostAttachmentCountWhenAttendeeStatusChanges()
    {
        var hostAttachmentId = AttachmentMockData.BundFutureApprovedKantonStGallenGuid;
        var attendeeContestDoiId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;

        var existingDoiCount = await GetDbEntity<DomainOfInfluenceAttachmentCount>(
            x => x.AttachmentId == hostAttachmentId && x.DomainOfInfluenceId == attendeeContestDoiId);
        var attachmentBeforeRemove = await GetDbEntity<Attachment>(a => a.Id == hostAttachmentId);
        var expectedRequiredCount = attachmentBeforeRemove.TotalRequiredCount - existingDoiCount.RequiredCount.GetValueOrDefault();
        var expectedRequiredForVoterListsCount = attachmentBeforeRemove.TotalRequiredForVoterListsCount - existingDoiCount.RequiredForVoterListsCount;

        // GemeindeArnegg stops being responsible for voting cards, so it is no longer a political
        // business attendee of anyone.
        await SetResponsibleForVotingCards(attendeeContestDoiId, false);
        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.GemeindeArneggGuid);

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts.AnyAsync(x => x.Id == existingDoiCount.Id)))
            .Should().BeFalse();

        var attachmentAfterRemove = await GetDbEntity<Attachment>(a => a.Id == hostAttachmentId);
        attachmentAfterRemove.TotalRequiredCount.Should().Be(expectedRequiredCount);
        attachmentAfterRemove.TotalRequiredForVoterListsCount.Should().Be(expectedRequiredForVoterListsCount);

        // becoming responsible for voting cards again re-adds the entry on KantonStGallen's attachment
        await SetResponsibleForVotingCards(attendeeContestDoiId, true);
        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.GemeindeArneggGuid);

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .AnyAsync(x => x.AttachmentId == hostAttachmentId && x.DomainOfInfluenceId == attendeeContestDoiId)))
            .Should().BeTrue();
    }

    [Fact]
    public async Task SyncForBasisDomainOfInfluenceShouldAdjustTotalsWhenRemovingOwnedAttachmentCount()
    {
        var ownedAttachmentId = AttachmentMockData.BundFutureApprovedGemeindeArneggGuid;
        var attendeeDoiId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid;

        // Add Gossau's counting circle to Doi Gemeinde Arnegg.
        // Doi StadtGossau becomes a "main voting card attendee" of GemeindeArnegg, so Doi StadtGossau
        // gets a DoiCount for Gemeinde Arnegg attachments.
        await UpdateCountingCircles(DomainOfInfluenceMockData.GemeindeArneggGuid, new[] { CountingCircleMockData.GemeindeArneggGuid, CountingCircleMockData.StadtGossauGuid });
        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.GemeindeArneggGuid);

        var addedDoiCount = await GetDbEntity<DomainOfInfluenceAttachmentCount>(
            x => x.AttachmentId == ownedAttachmentId && x.DomainOfInfluenceId == attendeeDoiId);

        await RunOnDb(async db =>
        {
            var tracked = await db.DomainOfInfluenceAttachmentCounts.AsTracking().SingleAsync(x => x.Id == addedDoiCount.Id);
            tracked.RequiredCount = 25;
            tracked.RequiredForVoterListsCount = 5;
            await db.SaveChangesAsync();
        });
        await RunScoped<AttachmentRepo>(repo => repo.UpdateTotalCountsForDomainOfInfluence(attendeeDoiId));

        var attachmentBeforeRemove = await GetDbEntity<Attachment>(a => a.Id == ownedAttachmentId);
        var expectedRequiredCount = attachmentBeforeRemove.TotalRequiredCount - 25;
        var expectedRequiredForVoterListsCount = attachmentBeforeRemove.TotalRequiredForVoterListsCount - 5;

        // move the counting circle back off GemeindeArnegg: StadtGossau is no longer an attendee
        await UpdateCountingCircles(DomainOfInfluenceMockData.GemeindeArneggGuid, new[] { CountingCircleMockData.GemeindeArneggGuid });
        await SyncForBasisDomainOfInfluence(DomainOfInfluenceMockData.GemeindeArneggGuid);

        (await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .AnyAsync(x => x.Id == addedDoiCount.Id)))
            .Should().BeFalse();

        var attachmentAfterRemove = await GetDbEntity<Attachment>(a => a.Id == ownedAttachmentId);
        attachmentAfterRemove.TotalRequiredCount.Should().Be(expectedRequiredCount);
        attachmentAfterRemove.TotalRequiredForVoterListsCount.Should().Be(expectedRequiredForVoterListsCount);
    }

    private Task<bool> AttachmentExists(Guid attachmentId) => RunOnDb(db => db.Attachments.AnyAsync(a => a.Id == attachmentId));

    private Task SetResponsibleForVotingCards(Guid contestDoiId, bool responsibleForVotingCards) =>
        RunOnDb(async db =>
        {
            var contestDoi = await db.ContestDomainOfInfluences.AsTracking().SingleAsync(x => x.Id == contestDoiId);
            contestDoi.ResponsibleForVotingCards = responsibleForVotingCards;
            await db.SaveChangesAsync();
        });

    private Task SyncForBasisDomainOfInfluence(Guid basisDoiId) =>
        RunScoped<AttachmentBuilder>(builder => builder.SyncForBasisDomainOfInfluence(basisDoiId));

    private Task UpdateCountingCircles(Guid basisDoiId, Guid[] countingCircleIds) =>
        RunScoped<DomainOfInfluenceCountingCircleBuilder>(builder => builder.UpdateDomainOfInfluenceCountingCircles(basisDoiId, countingCircleIds.ToList()));
}
