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

public class VoterListBuilderTest : BaseWriteableDbTest
{
    private readonly VoterListBuilder _voterListBuilder;

    public VoterListBuilderTest(TestApplicationFactory factory)
        : base(factory)
    {
        _voterListBuilder = GetService<VoterListBuilder>();
    }

    [Fact]
    public async Task CleanUpShouldIgnorePoliticalAssemblyWithNoPoliticalBusinessEntry()
    {
        await _voterListBuilder.CleanUp(new[] { ContestMockData.PoliticalAssemblyBundFutureApprovedGuid });
        (await VoterListExists(VoterListMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid)).Should().BeTrue();
    }

    [Fact]
    public async Task CleanUpShouldDeleteIfNoPoliticalBusinessEntryExists()
    {
        var voterListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissElectoralRegisterGuid;

        await _voterListBuilder.CleanUp(new[] { ContestMockData.BundFutureApprovedGuid });
        (await VoterListExists(voterListId)).Should().BeTrue();

        await RunOnDb(async db =>
        {
            var entries = await db.PoliticalBusinessVoterListEntries
            .Where(x => x.VoterListId == voterListId)
                .ToListAsync();

            db.PoliticalBusinessVoterListEntries.RemoveRange(entries);
            await db.SaveChangesAsync();
        });

        await _voterListBuilder.CleanUp(new[] { ContestMockData.BundFutureApprovedGuid });
        (await VoterListExists(voterListId)).Should().BeFalse();
    }

    [Fact]
    public async Task CleanUpShouldDeleteIfNoVoterListStepExists()
    {
        await RunOnDb(async db =>
        {
            var stepState = await db.StepStates
                .SingleAsync(s => s.DomainOfInfluenceId == DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid && s.Step == Step.VoterLists);

            db.StepStates.Remove(stepState);
            await db.SaveChangesAsync();
        });

        await _voterListBuilder.CleanUp(new[] { ContestMockData.PoliticalAssemblyBundFutureApprovedGuid });
        (await VoterListExists(VoterListMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggSwissGuid)).Should().BeFalse();
    }

    private Task<bool> VoterListExists(Guid voterListId) => RunOnDb(db => db.VoterLists.AnyAsync(a => a.Id == voterListId));
}
