// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.CountingCircleTests;

public class CountingCircleProcessorTest : BaseWriteableDbTest
{
    public CountingCircleProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CountingCircleCreated()
    {
        var id = Guid.Parse("508b9e65-c051-463e-aca7-0fa457babe25");
        var eventData = new CountingCircleCreated()
        {
            CountingCircle = new CountingCircleEventData()
            {
                Id = id.ToString(),
                Name = "St.Gallen Tablat (neu)",
                Bfs = "2345",
                EVoting = true,
            },
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate doi itself
        var doi = await RunOnDb(db => db.CountingCircles.FirstAsync(x => x.Id == id));
        doi.ShouldMatchChildSnapshot("cc");

        // validate created in contests
        var contestsInTestPhase = await RunOnDb(db => db.Contests.WhereInTestingPhase().CountAsync());
        var contestCcs = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.BasisCountingCircleId == id)
            .OrderBy(x => x.ContestId)
            .ToListAsync());
        contestCcs.Should().HaveCount(contestsInTestPhase);
        contestCcs.ShouldMatchChildSnapshot("contest_ccs");

        // validate assigned contest counting circles
        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.CountingCircle!.BasisCountingCircleId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");
    }

    [Fact]
    public async Task CountingCircleUpdated()
    {
        var originalName = CountingCircleMockData.StadtGossau.Name;
        var name = "Gossau updated";
        var id = CountingCircleMockData.StadtGossauGuid;
        var eventData = new CountingCircleUpdated
        {
            CountingCircle = new CountingCircleEventData
            {
                Id = id.ToString(),
                Name = name,
                Bfs = "1234",
                EVoting = true,
            },
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate CC itself
        var doi = await RunOnDb(db => db.CountingCircles.FirstAsync(x => x.Id == id));
        doi.ShouldMatchChildSnapshot("cc");

        // validate assigned domain of influences
        var doiCcs = await RunOnDb(db => db.DomainOfInfluenceCountingCircles
            .Where(x => x.CountingCircleId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        doiCcs.ShouldMatchChildSnapshot("doi_ccs");

        // validate updated in contests
        var contestsInTestPhase = await RunOnDb(db => db.Contests.WhereInTestingPhase().CountAsync());
        var contestCcs = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.BasisCountingCircleId == id)
            .OrderBy(x => x.ContestId)
            .Select(x => new { x.ContestId, x.BasisCountingCircleId, x.Name, x.EVoting })
            .ToListAsync());
        contestCcs.Count(x => x.Name == name).Should().Be(contestsInTestPhase);
        contestCcs.Count(x => x.Name == originalName).Should().Be(4);
        contestCcs.ShouldMatchChildSnapshot("contest_cc");
    }

    [Fact]
    public async Task CountingCirclesMergerActivated()
    {
        var id = CountingCircleMockData.StadtGossauGuid;
        var newId = Guid.Parse("bb0a8f0f-c533-4585-a96c-d0ee43f4aefc");
        var eventData = new CountingCirclesMergerActivated
        {
            Merger = new CountingCirclesMergerEventData
            {
                CopyFromCountingCircleId = id.ToString(),
                MergedCountingCircleIds = { id.ToString(), CountingCircleMockData.GemeindeArneggId },
                NewCountingCircle = new CountingCircleEventData
                {
                    Id = newId.ToString(),
                    Bfs = "4444",
                    Name = "merged cc",
                },
            },
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate CC itself
        var cc = await RunOnDb(db => db.CountingCircles.FirstAsync(x => x.Id == newId));
        cc.ShouldMatchChildSnapshot("doi");

        // validate assigned domain of influences
        var doiCcs = await RunOnDb(db => db.DomainOfInfluenceCountingCircles
            .Where(x => x.CountingCircleId == newId)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        doiCcs.ShouldMatchChildSnapshot("doi_ccs");

        // validate updated in contests
        var contestDois = await RunOnDb(db => db.ContestCountingCircles
            .Where(x => x.BasisCountingCircleId == newId)
            .OrderBy(x => x.ContestId)
            .Select(x => new { x.ContestId, x.BasisCountingCircleId, x.Name })
            .ToListAsync());
        contestDois.ShouldMatchChildSnapshot("contest_doi");

        // validate assigned contest counting circles
        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.CountingCircle!.BasisCountingCircleId == newId)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");
    }

    [Fact]
    public async Task CountingCircleDeleted()
    {
        var eventData = new CountingCircleDeleted()
        {
            CountingCircleId = CountingCircleMockData.StadtGossauId,
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        (await RunOnDb(db => db.CountingCircles.AnyAsync(x => x.Id == CountingCircleMockData.StadtGossauGuid)))
            .Should()
            .BeFalse();

        var contestsNotInTestingPhaseCount = await RunOnDb(db => db.Contests.WhereInTestingPhase(false).CountAsync());
        var gossauCount = await RunOnDb(db => db.ContestCountingCircles
            .CountAsync(x => x.BasisCountingCircleId == CountingCircleMockData.StadtGossauGuid));
        gossauCount.Should().Be(contestsNotInTestingPhaseCount);
    }
}
