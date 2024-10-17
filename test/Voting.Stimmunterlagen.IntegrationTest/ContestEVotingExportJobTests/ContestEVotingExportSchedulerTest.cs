// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.HostedServices;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestEVotingExportJobTests;

public class ContestEVotingExportSchedulerTest : BaseWriteableDbTest
{
    public ContestEVotingExportSchedulerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldScheduleJobs()
    {
        await RunOnDb(async db =>
        {
            var export = await db.ContestEVotingExportJobs.FirstOrDefaultAsync(x => x.ContestId == ContestMockData.BundFutureApprovedGuid);
            export!.State = ExportJobState.ReadyToRun;
            db.ContestEVotingExportJobs.Update(export);
            await db.SaveChangesAsync();
        });

        using var scope = GetService<IServiceScopeFactory>().CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<ContestEVotingExportScheduler>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        await scheduler.Run();
        var inReadyState = await RunOnDb(db => db.ContestEVotingExportJobs
            .WhereInState(ExportJobState.ReadyToRun)
            .WhereContestInTestingPhase()
            .AnyAsync());
        inReadyState.Should().BeFalse();
    }
}
