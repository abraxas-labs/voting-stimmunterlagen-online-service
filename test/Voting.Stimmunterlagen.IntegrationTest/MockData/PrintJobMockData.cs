// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class PrintJobMockData
{
    public static readonly Guid BundFutureApprovedGemeindeArneggReadyGuid = Guid.Parse("e4479a85-a97e-4fc5-80d3-92d177301c62");

    public static IEnumerable<PrintJob> All
    {
        get
        {
            yield return BundFutureApprovedGemeindeArneggReady;
        }
    }

    public static PrintJob BundFutureApprovedGemeindeArneggReady => new()
    {
        Id = BundFutureApprovedGemeindeArneggReadyGuid,
        State = PrintJobState.ReadyForProcess,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
    };

    public static async Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        // create mock data
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var doiIds = All.Select(x => x.DomainOfInfluenceId).ToList();

            var toDelete = await db.PrintJobs.Where(x => doiIds.Contains(x.DomainOfInfluenceId)).ToListAsync();

            db.PrintJobs.RemoveRange(toDelete);
            db.PrintJobs.AddRange(All);
            await db.SaveChangesAsync();
        });

        await runScoped(async sp =>
        {
            var builder = sp.GetRequiredService<PrintJobBuilder>();
            foreach (var contest in ContestMockData.All)
            {
                await builder.SyncForContest(contest.Id);
            }
        });
    }
}
