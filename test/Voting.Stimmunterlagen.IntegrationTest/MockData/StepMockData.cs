// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class StepMockData
{
    private static readonly List<(Guid, Step)> ApprovedSteps = new()
    {
        // Contest BundFuture
        (DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid, Step.PoliticalBusinessesApproval),

        // Contest BundFutureApproved
        (DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.PoliticalBusinessesApproval),
        (DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.LayoutVotingCardsContestManager),
        (DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.LayoutVotingCardsDomainOfInfluences),
        (DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, Step.ContestApproval),
        (DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid, Step.PoliticalBusinessesApproval),
    };

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var steps = await db.StepStates.AsTracking().ToListAsync();
            var stepsByDoiId = steps.GroupBy(x => x.DomainOfInfluenceId).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.Step));

            foreach (var (doiId, step) in ApprovedSteps)
            {
                stepsByDoiId[doiId][step].Approved = true;
            }

            await db.SaveChangesAsync();
        });
    }
}
