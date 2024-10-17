// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class ContestEVotingExportJobMockData
{
    public const string BundFutureApprovedId = "16887688-dc9d-4a9f-aa17-3e61f5505c28";

    public static readonly Guid BundFutureApprovedGuid = Guid.Parse(BundFutureApprovedId);

    private static ContestEVotingExportJob BundFutureApproved => new()
    {
        Id = BundFutureApprovedGuid,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        FileName = "file-name.zip",
        Completed = MockedClock.GetDate(5, 2),
        Started = MockedClock.GetDate(5),
        State = ExportJobState.Completed,
        FileHash = "WsoKmJdM8ACdxPXmvlKYpdUUNQZXPkC6vyVt4+0+i4yMFHQYHcahbb9MRxYn7teAlff0mue4kuqCt1CcKvpAVw==",
    };

    private static IEnumerable<ContestEVotingExportJob> All
    {
        get
        {
            yield return BundFutureApproved;
        }
    }

    public static async Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();

            var contestIds = All.Select(x => x.ContestId).ToList();
            var toDelete = await db.ContestEVotingExportJobs.Where(x => contestIds.Contains(x.ContestId)).ToListAsync();

            db.RemoveRange(toDelete);
            db.AddRange(All);
            await db.SaveChangesAsync();
        });
    }
}
