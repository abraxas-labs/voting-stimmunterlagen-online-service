// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Utils;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class PoliticalAssemblyProcessorTest : BaseWriteableDbTest
{
    public PoliticalAssemblyProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task PoliticalAssemblyCreated()
    {
        var id = Guid.Parse("ff0a5c73-c243-45d9-aedb-d7e320796836");

        (await HasVoterContestIndexSequence(id)).Should().BeFalse();

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(new PoliticalAssemblyCreated
        {
            PoliticalAssembly = new PoliticalAssemblyEventData
            {
                Id = id.ToString(),
                Date = MockedClock.GetTimestampDate(),
                Description = { LanguageUtil.MockAllLanguages("Political assembly 01") },
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
            },
        });

        var contest = await RunOnDb(
            db => db.Contests
                .Include(x => x.DomainOfInfluence)
                .Include(x => x.Translations)
                .SingleAsync(x => x.Id == id),
            Languages.French);
        contest.DomainOfInfluence!.ManagedContest = null;
        contest.DomainOfInfluence.Id = Guid.Empty;
        contest.DomainOfInfluenceId = Guid.Empty;
        foreach (var translation in contest.Translations!)
        {
            translation.Id = Guid.Empty;
            translation.Contest = null;
        }

        contest.ShouldMatchChildSnapshot("contest");

        var permissions = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.ContestId == id)
            .OrderBy(x => x.BasisDomainOfInfluenceId)
            .Select(x => new { x.Name, x.BasisDomainOfInfluenceId, x.SecureConnectId, x.Role })
            .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");

        var contestCountingCircles = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.ContestId == contest.Id)
            .OrderBy(x => x.BasisCountingCircleId)
            .Select(x => new { x.Name, x.BasisCountingCircleId, x.Bfs })
            .ToListAsync());
        contestCountingCircles.ShouldMatchChildSnapshot("counting-circles");

        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluence!.ContestId == contest.Id)
            .OrderBy(x => x.CountingCircleId)
            .ThenBy(x => x.DomainOfInfluenceId)
            .Select(x => new { x.CountingCircleId, x.DomainOfInfluenceId })
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");

        var steps = await RunOnDb(db => db.StepStates
            .Where(x => x.DomainOfInfluence!.ContestId == id)
            .OrderBy(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId)
            .ThenBy(x => x.Step)
            .Select(x => new { x.Step, x.Approved, x.DomainOfInfluence!.BasisDomainOfInfluenceId, x.DomainOfInfluence.Name })
            .ToListAsync());
        steps.ShouldMatchChildSnapshot("steps");

        var layouts = await RunOnDb(db => db.ContestVotingCardLayouts
            .Where(x => x.ContestId == id)
            .OrderBy(x => x.VotingCardType)
            .Select(x => new { x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        layouts.ShouldMatchChildSnapshot("contest-layouts");

        var doiLayouts = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .Where(x => x.DomainOfInfluence!.ContestId == id)
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ThenBy(x => x.VotingCardType)
            .Select(x => new { x.DomainOfInfluence!.Name, x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        doiLayouts.ShouldMatchChildSnapshot("doi-layouts");

        (await HasVoterContestIndexSequence(id)).Should().BeTrue();
    }

    [Fact]
    public async Task PoliticalAssemblyUpdated()
    {
        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(new PoliticalAssemblyUpdated
        {
            PoliticalAssembly = new PoliticalAssemblyEventData
            {
                Id = ContestMockData.PoliticalAssemblyBundFutureApprovedId,
                Date = MockedClock.GetTimestampDate(),
                Description = { LanguageUtil.MockAllLanguages("Political assembly updated") },
                DomainOfInfluenceId = DomainOfInfluenceMockData.KantonStGallenId,
            },
        });

        var contest = await RunOnDb(
            db => db.Contests
                .Include(x => x.Translations)
                .FirstAsync(x => x.Id == ContestMockData.PoliticalAssemblyBundFutureApprovedGuid),
            Languages.German);
        contest.DomainOfInfluenceId = Guid.Empty;
        foreach (var translation in contest.Translations!)
        {
            translation.Id = Guid.Empty;
            translation.Contest = null;
        }

        contest.ShouldMatchChildSnapshot("contest");

        var permissions = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.ContestId == contest.Id)
            .OrderBy(x => x.BasisDomainOfInfluenceId)
            .Select(x => new { x.Name, x.BasisDomainOfInfluenceId, x.SecureConnectId, x.Role })
            .ToListAsync());
        permissions.ShouldMatchChildSnapshot("permissions");

        var contestCountingCircles = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.ContestId == contest.Id)
            .OrderBy(x => x.BasisCountingCircleId)
            .Select(x => new { x.Name, x.BasisCountingCircleId, x.Bfs })
            .ToListAsync());
        contestCountingCircles.ShouldMatchChildSnapshot("counting-circles");

        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluence!.ContestId == contest.Id)
            .OrderBy(x => x.CountingCircleId)
            .ThenBy(x => x.DomainOfInfluenceId)
            .Select(x => new { x.CountingCircleId, x.DomainOfInfluenceId })
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");

        var steps = await RunOnDb(db => db.StepStates
            .Where(x => x.DomainOfInfluence!.ContestId == contest.Id)
            .OrderBy(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId)
            .ThenBy(x => x.Step)
            .Select(x => new { x.Step, x.Approved, x.DomainOfInfluence!.BasisDomainOfInfluenceId, x.DomainOfInfluence.Name })
            .ToListAsync());
        steps.ShouldMatchChildSnapshot("steps");

        var layouts = await RunOnDb(db => db.ContestVotingCardLayouts
            .Where(x => x.ContestId == contest.Id)
            .OrderBy(x => x.VotingCardType)
            .Select(x => new { x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        layouts.ShouldMatchChildSnapshot("contest-layouts");

        var doiLayouts = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .Where(x => x.DomainOfInfluence!.ContestId == contest.Id)
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ThenBy(x => x.VotingCardType)
            .Select(x => new { x.DomainOfInfluence!.Name, x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        doiLayouts.ShouldMatchChildSnapshot("doi-layouts");
    }

    [Fact]
    public async Task PoliticalAssemblyDeleted()
    {
        var id = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid;

        (await HasVoterContestIndexSequence(id)).Should().BeTrue();

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(
            new PoliticalAssemblyDeleted
            {
                PoliticalAssemblyId = id.ToString(),
            });

        (await RunOnDb(db => db.Contests.AnyAsync(x => x.Id == id)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.ContestDomainOfInfluences.AnyAsync(x => x.ContestId == id)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.ContestCountingCircles.AnyAsync(x => x.ContestId == id)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.StepStates.AnyAsync(x => x.DomainOfInfluence!.ContestId == id)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.ContestVotingCardLayouts.AnyAsync(x => x.ContestId == id)))
            .Should()
            .BeFalse();

        (await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts.AnyAsync(x => x.DomainOfInfluence!.ContestId == id)))
            .Should()
            .BeFalse();

        (await HasVoterContestIndexSequence(id)).Should().BeFalse();
    }

    [Fact]
    public Task PoliticalAssemblyPastLocked()
    => TestStateChange(new PoliticalAssemblyPastLocked { PoliticalAssemblyId = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid.ToString() }, ContestState.PastLocked);

    [Fact]
    public async Task PoliticalAssemblyArchived()
    {
        var id = ContestMockData.PoliticalAssemblyBundFutureApprovedGuid;
        (await HasVoterContestIndexSequence(id)).Should().BeTrue();
        await TestStateChange(new PoliticalAssemblyArchived { PoliticalAssemblyId = id.ToString() }, ContestState.Archived);
        (await HasVoterContestIndexSequence(id)).Should().BeFalse();
    }

    private async Task<bool> HasVoterContestIndexSequence(Guid contestId)
    {
        var sequenceName = SequenceNames.BuildVoterContestIndexSequenceName(contestId);
        var count = await RunOnDb(async db =>
        {
            await db.Database.OpenConnectionAsync();
            try
            {
                await using var cmd = db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM pg_class WHERE relname = '{sequenceName}' AND relkind = 'S'";
                return (long)(await cmd.ExecuteScalarAsync())!;
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        });

        return count > 0;
    }

    private async Task TestStateChange<T>(T eventData, ContestState targetState)
    where T : IMessage<T>
    {
        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.PoliticalAssemblyBundFutureApprovedGuid));
        contest.State.Should().Be(ContestState.TestingPhase);
        contest.TestingPhaseEnded.Should().BeFalse();

        await TestEventPublisher.PublishTwice(eventData);

        contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.PoliticalAssemblyBundFutureApprovedGuid));
        contest.State.Should().Be(targetState);
        contest.TestingPhaseEnded.Should().Be(targetState > ContestState.TestingPhase);
    }
}
