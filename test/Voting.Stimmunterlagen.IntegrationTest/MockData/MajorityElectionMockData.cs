// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class MajorityElectionMockData
{
    public const string BundFuture1Id = "56fec37a-a5a4-42ac-9c2e-2d02f74c3333";
    public const string BundFuture2Id = "8b9c1b7a-c519-4cf6-a1cb-f5789384a036";
    public const string BundFuture3Id = "49abd709-5c23-4d31-936f-f36936c34f21";
    public const string BundFuture4Id = "42b77898-3f77-4a10-a4d3-242198a26838";
    public const string BundFuture5Id = "6a30f10e-20c2-4bfd-aaa5-b058a34fb2be";
    public const string BundFutureStadtGossau1Id = "8084119a-2315-4e62-b2eb-4e18a0e8789c";
    public const string BundFutureGemeindeArnegg1Id = "4cd49a49-d9ba-4a2e-845f-d432a3401ac1";
    public const string BundFutureApproved1Id = "0a6f0b72-bd98-4068-a339-284e7bdd37ab";
    public const string SchulgemeindeAndwilArneggFuture1Id = "426008a8-b535-4730-8803-cd6d655614a3";
    public const string BundFutureApprovedKantonStGallen1Id = "4e71e6dd-f040-4862-bd0f-fcdf69aef68b";
    public const string BundFutureApprovedStadtGossau1Id = "9f93d743-524e-4ea9-aa81-6b3e79c84a6a";
    public const string BundFutureApprovedGemeindeArnegg1Id = "d80617ea-36b9-4ad3-a767-59310c94a4cc";
    public const string BundFutureApprovedAuslandschweizerId = "8f686009-eb85-46df-b749-52245fc0160a";

    public static readonly Guid BundFuture1Guid = Guid.Parse(BundFuture1Id);
    public static readonly Guid BundFuture2Guid = Guid.Parse(BundFuture2Id);
    public static readonly Guid BundFuture3Guid = Guid.Parse(BundFuture3Id);
    public static readonly Guid BundFuture4Guid = Guid.Parse(BundFuture4Id);
    public static readonly Guid BundFuture5Guid = Guid.Parse(BundFuture5Id);
    public static readonly Guid BundFutureStadtGossau1Guid = Guid.Parse(BundFutureStadtGossau1Id);
    public static readonly Guid BundFutureGemeindeArnegg1Guid = Guid.Parse(BundFutureGemeindeArnegg1Id);
    public static readonly Guid BundFutureApproved1Guid = Guid.Parse(BundFutureApproved1Id);
    public static readonly Guid SchulgemeindeAndwilArneggFuture1Guid = Guid.Parse(SchulgemeindeAndwilArneggFuture1Id);
    public static readonly Guid BundFutureApprovedKantonStGallen1Guid = Guid.Parse(BundFutureApprovedKantonStGallen1Id);
    public static readonly Guid BundFutureApprovedStadtGossau1Guid = Guid.Parse(BundFutureApprovedStadtGossau1Id);
    public static readonly Guid BundFutureApprovedGemeindeArnegg1Guid = Guid.Parse(BundFutureApprovedGemeindeArnegg1Id);
    public static readonly Guid BundFutureApprovedAuslandschweizerGuid = Guid.Parse(BundFutureApprovedAuslandschweizerId);

    public static PoliticalBusiness BundFuture1 => new()
    {
        Id = BundFuture1Guid,
        PoliticalBusinessNumber = "001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection 001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection 001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture2 => new()
    {
        Id = BundFuture2Guid,
        PoliticalBusinessNumber = "002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection 002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection 002 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture3 => new()
    {
        Id = BundFuture3Guid,
        PoliticalBusinessNumber = "003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection 003 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture4 => new()
    {
        Id = BundFuture4Guid,
        PoliticalBusinessNumber = "004",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection 004 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture5 => new()
    {
        Id = BundFuture5Guid,
        PoliticalBusinessNumber = "005",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection 005 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection 005 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureStadtGossau1 => new()
    {
        Id = BundFutureStadtGossau1Guid,
        PoliticalBusinessNumber = "M001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection M001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection M001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureGemeindeArnegg1 => new()
    {
        Id = BundFutureGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "M001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future MajorityElection M001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future MajorityElection M001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApproved1 => new()
    {
        Id = BundFutureApproved1Guid,
        PoliticalBusinessNumber = "101",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved MajorityElection 101 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved MajorityElection 101 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness SchulgemeindeAndwilArneggFuture1 => new()
    {
        Id = SchulgemeindeAndwilArneggFuture1Guid,
        PoliticalBusinessNumber = "1_1",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "SchulgemeindeAndwilArneggFuture (Schulpräsident) MajorityElection 1_1 (short)",
            (t, x) => t.OfficialDescription = x,
            "SchulgemeindeAndwilArneggFuture (Schulpräsident) MajorityElection 1_1 (official)"),
        Active = true,
        ContestId = ContestMockData.SchulgemeindeAndwilArneggFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
    };

    public static PoliticalBusiness BundFutureApprovedKantonStGallen1 => new()
    {
        Id = BundFutureApprovedKantonStGallen1Guid,
        PoliticalBusinessNumber = "M001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved MajorityElection M001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved MajorityElection M001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApprovedStadtGossau1 => new()
    {
        Id = BundFutureApprovedStadtGossau1Guid,
        PoliticalBusinessNumber = "M002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved MajorityElection M002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved MajorityElection M002 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApprovedGemeindeArnegg1 => new()
    {
        Id = BundFutureApprovedGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "M003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved MajorityElection M003 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved MajorityElection M003 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApprovedAuslandschweizer => new()
    {
        Id = BundFutureApprovedAuslandschweizerGuid,
        PoliticalBusinessNumber = "M004",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved MajorityElection M004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved MajorityElection M004 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedAuslandschweizerGuid,
        PoliticalBusinessType = PoliticalBusinessType.MajorityElection,
        Approved = true,
    };

    public static IEnumerable<PoliticalBusiness> All
    {
        get
        {
            yield return BundFuture1;
            yield return BundFuture2;
            yield return BundFuture3;
            yield return BundFuture4;
            yield return BundFuture5;
            yield return BundFutureStadtGossau1;
            yield return BundFutureGemeindeArnegg1;
            yield return BundFutureApproved1;
            yield return SchulgemeindeAndwilArneggFuture1;
            yield return BundFutureApprovedKantonStGallen1;
            yield return BundFutureApprovedStadtGossau1;
            yield return BundFutureApprovedGemeindeArnegg1;
            yield return BundFutureApprovedAuslandschweizer;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.PoliticalBusinesses.AddRange(All);
            await db.SaveChangesAsync();

            var permissionBuilder = sp.GetRequiredService<PoliticalBusinessPermissionBuilder>();
            await permissionBuilder.UpdatePermissionsForPoliticalBusinessesInTestingPhase();
        });
    }
}
