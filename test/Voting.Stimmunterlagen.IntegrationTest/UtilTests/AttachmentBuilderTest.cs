// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data.Models;
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

    private Task<bool> AttachmentExists(Guid attachmentId) => RunOnDb(db => db.Attachments.AnyAsync(a => a.Id == attachmentId));
}
