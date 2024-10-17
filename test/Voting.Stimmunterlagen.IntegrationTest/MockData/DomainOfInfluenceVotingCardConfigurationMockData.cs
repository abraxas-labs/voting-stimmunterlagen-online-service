// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class DomainOfInfluenceVotingCardConfigurationMockData
{
    public static IEnumerable<DomainOfInfluenceVotingCardConfiguration> All
    {
        get
        {
            yield return BundFutureGemeindeArnegg;
            yield return BundFutureApprovedGemeindeArnegg;
        }
    }

    private static DomainOfInfluenceVotingCardConfiguration BundFutureGemeindeArnegg => new()
    {
        Id = Guid.Parse("cc518bea-40c5-488f-ac47-531380c3a583"),
        Groups = new[] { VotingCardGroup.Language },
        Sorts = new[] { VotingCardSort.Name },
        SampleCount = 25,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
    };

    private static DomainOfInfluenceVotingCardConfiguration BundFutureApprovedGemeindeArnegg => new()
    {
        Id = Guid.Parse("69cbc888-aa4c-437f-92b0-8cadb24baf13"),
        Groups = new[] { VotingCardGroup.Language },
        Sorts = new[] { VotingCardSort.Name },
        SampleCount = 25,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
    };

    public static async Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        // create mock data
        await runScoped(sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.DomainOfInfluenceVotingCardConfigurations.AddRange(All);
            return db.SaveChangesAsync();
        });

        // seed missing configurations
        await runScoped(async sp =>
        {
            var builder = sp.GetRequiredService<DomainOfInfluenceVotingCardConfigurationBuilder>();
            foreach (var contest in ContestMockData.All)
            {
                await builder.SyncForContest(contest.Id);
            }
        });
    }
}
