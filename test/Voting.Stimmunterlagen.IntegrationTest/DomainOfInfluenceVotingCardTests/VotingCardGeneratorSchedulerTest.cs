// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.HostedServices;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardTests;

public class VotingCardGeneratorSchedulerTest : BaseWriteableDbTest
{
    public VotingCardGeneratorSchedulerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldScheduleJobs()
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<VotingCardGeneratorScheduler>();
        await scheduler.Run();
        var inReadyState = await RunOnDb(db => db.VotingCardGeneratorJobs
            .WhereInState(VotingCardGeneratorJobState.Ready)
            .WhereContestIsInTestingPhase()
            .AnyAsync());
        inReadyState.Should().BeFalse();
    }
}
