// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Extensions;

public class ContestQueryableExtensionsTest
{
    [Fact]
    public void HasContestWhereContestNotLocked()
    {
        var queryable = BuildHasContestQueryable(
            ContestState.Active,
            ContestState.Active,
            ContestState.TestingPhase,
            ContestState.TestingPhase,
            ContestState.PastUnlocked,
            ContestState.PastLocked,
            ContestState.Archived);

        var filteredResult = queryable.WhereContestNotLocked().ToList();
        filteredResult.Count.Should().Be(5);
        filteredResult.Any(r => r.Contest!.State == ContestState.PastLocked).Should().BeFalse();
        filteredResult.Any(r => r.Contest!.State == ContestState.Archived).Should().BeFalse();
    }

    [Fact]
    public void ContestWhereNotLocked()
    {
        var queryable = BuildContestQueryable(
            ContestState.Active,
            ContestState.TestingPhase,
            ContestState.PastUnlocked,
            ContestState.PastLocked,
            ContestState.Archived);

        var filteredResult = queryable.WhereNotLocked().ToList();
        filteredResult.Count.Should().Be(3);
        filteredResult.Any(r => r.State == ContestState.PastLocked).Should().BeFalse();
        filteredResult.Any(r => r.State == ContestState.Archived).Should().BeFalse();
    }

    private IQueryable<IHasContest> BuildHasContestQueryable(params ContestState[] states)
        => states.Select(s => new ContestVotingCardLayout { Contest = new() { State = s } }).AsQueryable();

    private IQueryable<Contest> BuildContestQueryable(params ContestState[] states)
        => states.Select(s => new Contest { State = s }).AsQueryable();
}
