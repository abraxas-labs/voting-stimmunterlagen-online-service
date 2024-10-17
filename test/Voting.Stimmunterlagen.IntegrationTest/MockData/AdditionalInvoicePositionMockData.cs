// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class AdditionalInvoicePositionMockData
{
    public const string BundFutureApprovedArnegg1Id = "d7b1e1a4-c1a5-4e09-b54c-727532aa18a4";
    public const string BundFutureApprovedArnegg2Id = "249c8c97-22cd-4704-af7e-7452e2e40f6b";
    public const string BundFutureApprovedArneggOldId = "08ce9c56-78d1-4f4e-ab39-4f051d35b17d";

    public static readonly Guid BundFutureApprovedArnegg1Guid = Guid.Parse(BundFutureApprovedArnegg1Id);
    public static readonly Guid BundFutureApprovedArnegg2Guid = Guid.Parse(BundFutureApprovedArnegg2Id);
    public static readonly Guid BundFutureApprovedArneggOldGuid = Guid.Parse(BundFutureApprovedArneggOldId);

    public static IEnumerable<AdditionalInvoicePosition> All
    {
        get
        {
            yield return BundFutureApprovedArnegg1;
            yield return BundFutureApprovedArnegg2;
            yield return BundFutureApprovedArneggOld;
        }
    }

    private static AdditionalInvoicePosition BundFutureApprovedArnegg1 => new()
    {
        Id = BundFutureApprovedArnegg1Guid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        MaterialNumber = "1040.05.57",
        AmountCentime = 1975,
        Created = MockedClock.GetDate(1),
        CreatedBy = UserMockData.DruckverwalterUtz,
        Modified = MockedClock.GetDate(1),
        ModifiedBy = UserMockData.DruckverwalterUtz,
    };

    private static AdditionalInvoicePosition BundFutureApprovedArnegg2 => new()
    {
        Id = BundFutureApprovedArnegg2Guid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        MaterialNumber = "1040.05.62",
        AmountCentime = 3000,
        Created = MockedClock.GetDate(1, 1),
        CreatedBy = UserMockData.DruckverwalterUtz,
        Modified = MockedClock.GetDate(1, 2),
        ModifiedBy = UserMockData.DruckverwalterUtz,
    };

    private static AdditionalInvoicePosition BundFutureApprovedArneggOld => new()
    {
        Id = BundFutureApprovedArneggOldGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        MaterialNumber = "Zusatzsticker",
        AmountCentime = 225,
        Created = MockedClock.GetDate(3),
        CreatedBy = UserMockData.DruckverwalterUtz,
        Modified = MockedClock.GetDate(3),
        ModifiedBy = UserMockData.DruckverwalterUtz,
    };

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.AdditionalInvoicePositions.AddRange(All);
            await db.SaveChangesAsync();
        });
    }
}
