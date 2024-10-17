// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class ContestVotingCardLayoutMockData
{
    public static IEnumerable<ContestVotingCardLayout> All
    {
        get
        {
            yield return BundArchivedSwiss;
            yield return BundFutureApprovedSwiss;
            yield return BundFutureSwiss;
        }
    }

    private static ContestVotingCardLayout BundArchivedSwiss => new()
    {
        ContestId = ContestMockData.BundArchivedGuid,
        AllowCustom = true,
        TemplateId = DmDocServiceMock.TemplateSwiss.Id,
        VotingCardType = VotingCardType.Swiss,
    };

    private static ContestVotingCardLayout BundFutureApprovedSwiss => new()
    {
        ContestId = ContestMockData.BundFutureApprovedGuid,
        AllowCustom = true,
        TemplateId = DmDocServiceMock.TemplateSwiss.Id,
        VotingCardType = VotingCardType.Swiss,
    };

    private static ContestVotingCardLayout BundFutureSwiss => new()
    {
        ContestId = ContestMockData.BundFutureGuid,
        AllowCustom = true,
        TemplateId = DmDocServiceMock.TemplateSwiss.Id,
        VotingCardType = VotingCardType.Swiss,
    };

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var layouts = await db.ContestVotingCardLayouts.AsTracking().ToListAsync();
            var layoutsByContestAndVotingCardType = layouts.ToDictionary(x => (x.ContestId, x.VotingCardType));

            foreach (var layoutValues in All)
            {
                var layout = layoutsByContestAndVotingCardType[(layoutValues.ContestId, layoutValues.VotingCardType)];
                layout.AllowCustom = layoutValues.AllowCustom;
                layout.TemplateId = layoutValues.TemplateId;
            }

            await db.SaveChangesAsync();
        });
    }
}
