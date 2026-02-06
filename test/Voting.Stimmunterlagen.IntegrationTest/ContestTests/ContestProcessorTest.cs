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
using SharedProto = Abraxas.Voting.Basis.Shared.V1;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestTests;

public class ContestProcessorTest : BaseWriteableDbTest
{
    public ContestProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ContestCreated()
    {
        var id = Guid.Parse("47891324-93db-4032-94fd-7650a9262134");

        (await HasVoterContestIndexSequence(id)).Should().BeFalse();

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(new ContestCreated
        {
            Contest = new ContestEventData
            {
                Id = id.ToString(),
                Date = MockedClock.GetTimestampDate(),
                Description = { LanguageUtil.MockAllLanguages("Contest 01") },
                DomainOfInfluenceId = DomainOfInfluenceMockData.BundId,
                EVoting = true,
                State = SharedProto.ContestState.TestingPhase,
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
    public async Task ContestUpdated()
    {
        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(new ContestUpdated
        {
            Contest = new ContestEventData
            {
                Id = ContestMockData.BundFutureId,
                Date = MockedClock.GetTimestampDate(),
                Description = { LanguageUtil.MockAllLanguages("Contest updated") },
                DomainOfInfluenceId = DomainOfInfluenceMockData.KantonStGallenId,
                EVoting = true,
                State = SharedProto.ContestState.TestingPhase,
            },
        });

        var contest = await RunOnDb(
            db => db.Contests
                .Include(x => x.Translations)
                .FirstAsync(x => x.Id == ContestMockData.BundFutureGuid),
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
    public async Task ContestDeleted()
    {
        var id = ContestMockData.BundFutureGuid;

        (await HasVoterContestIndexSequence(id)).Should().BeTrue();

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(
            new ContestDeleted
            {
                ContestId = id.ToString(),
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
    public Task ContestTestingPhaseEnded()
        => TestStateChange(new ContestTestingPhaseEnded { ContestId = ContestMockData.BundFutureId }, ContestState.Active);

    [Fact]
    public Task ContestPastLocked()
        => TestStateChange(new ContestPastLocked { ContestId = ContestMockData.BundFutureId }, ContestState.PastLocked);

    [Fact]
    public Task ContestPastUnlocked()
        => TestStateChange(new ContestPastUnlocked { ContestId = ContestMockData.BundFutureId }, ContestState.PastUnlocked);

    [Fact]
    public async Task ContestArchived()
    {
        var id = ContestMockData.BundFutureGuid;
        (await HasVoterContestIndexSequence(id)).Should().BeTrue();
        await TestStateChange(new ContestArchived { ContestId = id.ToString() }, ContestState.Archived);
        (await HasVoterContestIndexSequence(id)).Should().BeFalse();
    }

    [Fact]
    [Obsolete("contest counting circle options are deprecated")]
    public async Task ContestCountingCircleOptionsUpdated()
    {
        var contestId = ContestMockData.BundFutureGuid;
        await TestEventPublisher.Publish(new ContestCountingCircleOptionsUpdated()
        {
            ContestId = contestId.ToString(),
            Options =
            {
                new ContestCountingCircleOptionEventData
                {
                    CountingCircleId = CountingCircleMockData.StadtStGallenId,
                    EVoting = true,
                },
                new ContestCountingCircleOptionEventData
                {
                    CountingCircleId = CountingCircleMockData.StadtGossauId,
                    EVoting = true,
                },
            },
        });

        var contestCountingCircles = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.ContestId == contestId)
            .OrderBy(x => x.BasisCountingCircleId)
            .Select(x => new { x.Name, x.BasisCountingCircleId, x.Bfs, x.EVoting })
            .ToListAsync());
        contestCountingCircles.ShouldMatchChildSnapshot("counting-circles");
    }

    [Fact]
    public async Task UpdateContestShouldChangeMinorFlag()
    {
        var contestId = ContestMockData.PoliticalAssemblyBundFutureApprovedId;
        var domainOfInfluenceId = DomainOfInfluenceMockData.GemeindeArneggId;
        var minorCount = await RunOnDb(db => db.Voters
            .Where(x => x.ContestId == Guid.Parse(contestId) && x.IsMinor == true)
            .CountAsync());
        minorCount.Should().Be(0);
        await TestEventPublisher.PublishTwice(new ContestUpdated
        {
            Contest = new ContestEventData
            {
                Id = contestId,
                Date = MockedClock.GetTimestampDate(-6000),
                Description = { LanguageUtil.MockAllLanguages("Contest 01") },
                DomainOfInfluenceId = domainOfInfluenceId,
                EVoting = true,
                State = SharedProto.ContestState.TestingPhase,
            },
        });
        var minorCountAfter = await RunOnDb(db => db.Voters
            .Where(x => x.ContestId == Guid.Parse(contestId) && x.IsMinor == true)
            .CountAsync());
        minorCountAfter.Should().Be(1);
    }

    private async Task TestStateChange<T>(T eventData, ContestState targetState)
        where T : IMessage<T>
    {
        var contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.State.Should().Be(ContestState.TestingPhase);
        contest.TestingPhaseEnded.Should().BeFalse();

        await TestEventPublisher.PublishTwice(eventData);

        contest = await RunOnDb(db => db.Contests.SingleAsync(x => x.Id == ContestMockData.BundFutureGuid));
        contest.State.Should().Be(targetState);
        contest.TestingPhaseEnded.Should().Be(targetState > ContestState.TestingPhase);
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
}
