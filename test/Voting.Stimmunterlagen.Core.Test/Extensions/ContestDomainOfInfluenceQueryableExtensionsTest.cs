// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Extensions;

public class ContestDomainOfInfluenceQueryableExtensionsTest
{
    [Fact]
    public void HasContestDomainOfInfluenceWhereContestNotLocked()
    {
        var queryable = BuildHasContestDoiQueryable(
            ContestState.Active,
            ContestState.Active,
            ContestState.TestingPhase,
            ContestState.TestingPhase,
            ContestState.PastUnlocked,
            ContestState.PastLocked,
            ContestState.Archived);

        var filteredResult = queryable.WhereContestIsNotLocked().ToList();
        filteredResult.Count.Should().Be(5);
        filteredResult.Any(r => r.DomainOfInfluence!.Contest!.State == ContestState.PastLocked).Should().BeFalse();
        filteredResult.Any(r => r.DomainOfInfluence!.Contest!.State == ContestState.Archived).Should().BeFalse();
    }

    private IQueryable<IHasContestDomainOfInfluence> BuildHasContestDoiQueryable(params ContestState[] states)
        => states.Select(s => new Attachment { DomainOfInfluence = new() { Contest = new() { State = s } } }).AsQueryable();
}
