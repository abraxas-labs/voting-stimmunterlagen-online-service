// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class SecondaryMajorityElectionMockData
{
    public const string BundFuture51Id = "be0af996-5c16-4abd-8313-eb3b0d9ad29b";

    public static readonly Guid BundFuture51Guid = Guid.Parse(BundFuture51Id);

    public static PoliticalBusiness BundFuture5_1 => new()
    {
        Id = BundFuture51Guid,
        PoliticalBusinessNumber = "005.1",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future SecondaryMajorityElection 005.1 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future SecondaryMajorityElection 005.1 (official)"),
        Active = true,
        PoliticalBusinessType = PoliticalBusinessType.SecondaryMajorityElection,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
    };

    public static IEnumerable<PoliticalBusiness> All
    {
        get
        {
            yield return BundFuture5_1;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        var all = All.ToList();
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.PoliticalBusinesses.AddRange(all);
            await db.SaveChangesAsync();

            var permissionBuilder = sp.GetRequiredService<PoliticalBusinessPermissionBuilder>();
            await permissionBuilder.UpdatePermissionsForPoliticalBusinessesInTestingPhase();
        });
    }
}
