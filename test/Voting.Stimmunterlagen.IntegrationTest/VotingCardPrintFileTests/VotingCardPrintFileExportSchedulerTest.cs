// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.HostedServices;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardPrintFileTests;

public class VotingCardPrintFileExportSchedulerTest : BaseWriteableDbTest
{
    public VotingCardPrintFileExportSchedulerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldScheduleJobs()
    {
        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var authStore = scope.ServiceProvider.GetRequiredService<IAuthStore>();
        authStore.SetValues("test-token", "test", "test", Enumerable.Empty<string>());
        var scheduler = scope.ServiceProvider.GetRequiredService<VotingCardPrintFileExportScheduler>();
        await scheduler.Run();
        var inReadyState = await RunOnDb(db => db.VotingCardPrintFileExportJobs
            .WhereInState(ExportJobState.ReadyToRun)
            .WhereContestInTestingPhase()
            .AnyAsync());
        inReadyState.Should().BeFalse();
    }
}
