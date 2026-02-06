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

public static class VoteMockData
{
    public const string BundArchivedGemeindeArnegg1Id = "87732104-e226-4e74-9311-f86f1ee56b7d";
    public const string BundArchivedNotApprovedGemeindeArnegg1Id = "2516ed87-6de0-464d-a923-f82a00cfb6f1";
    public const string BundFuture1Id = "6f7b5c83-fb9c-4177-bbec-0f893cece402";
    public const string BundFuture2Id = "4cabfa9d-4956-4bea-96ea-41ac8b780dfd";
    public const string BundFuture3Id = "494f26d5-ed3c-4c08-b4a6-c45b4f2a39e1";
    public const string BundFuture4Id = "f13ce18a-164f-495a-aefc-9b47aa702bde";
    public const string BundFuture5Id = "9be261b8-7dc3-4b9e-b17c-5461a3b8e3b6";
    public const string BundFuture6Id = "5c666612-81c9-4e04-af29-e12d299c2291";
    public const string BundFutureGemeindeArnegg1Id = "f2a9182a-faba-4756-9211-59d19c63102c";
    public const string SchulgemeindeAndwilArneggFuture1Id = "0fd1d1fc-6250-4f2c-bfbe-eb3fd224e9f3";
    public const string BundFutureApproved1Id = "fa3834c8-dd4a-4d75-80ba-fe66f0f46d30";
    public const string BundFutureApproved2Id = "b17c5b4a-e9e9-4a6a-8083-352426a528f9";
    public const string BundFutureApprovedKantonStGallen1Id = "97aa25ce-0d1c-4330-8d99-46b59bfea4b1";
    public const string BundFutureApprovedStadtUzwil1Id = "551a9eda-8457-4c9d-9a86-18214b05711d";
    public const string BundFutureApprovedStadtGossau1Id = "6983d26b-dff8-4015-8601-38b310bc0e64";
    public const string BundFutureApprovedGemeindeArnegg1Id = "a9ffc699-3542-45df-8bac-febea9f60c1a";
    public const string BundFutureApprovedZweckverbandGossauId = "5ef5651c-6662-4221-b073-1cd8b4d42237";
    public const string BundFutureApprovedKirchgemeindeArneggId = "3de78256-89c4-4a28-98d5-ba3625d173e8";

    public static readonly Guid BundArchivedGemeindeArnegg1Guid = Guid.Parse(BundArchivedGemeindeArnegg1Id);
    public static readonly Guid BundArchivedNotApprovedGemeindeArnegg1Guid = Guid.Parse(BundArchivedNotApprovedGemeindeArnegg1Id);
    public static readonly Guid BundFuture1Guid = Guid.Parse(BundFuture1Id);
    public static readonly Guid BundFuture2Guid = Guid.Parse(BundFuture2Id);
    public static readonly Guid BundFuture3Guid = Guid.Parse(BundFuture3Id);
    public static readonly Guid BundFuture4Guid = Guid.Parse(BundFuture4Id);
    public static readonly Guid BundFuture5Guid = Guid.Parse(BundFuture5Id);
    public static readonly Guid BundFuture6Guid = Guid.Parse(BundFuture6Id);
    public static readonly Guid BundFutureGemeindeArnegg1Guid = Guid.Parse(BundFutureGemeindeArnegg1Id);
    public static readonly Guid SchulgemeindeAndwilArneggFuture1Guid = Guid.Parse(SchulgemeindeAndwilArneggFuture1Id);
    public static readonly Guid BundFutureApproved1Guid = Guid.Parse(BundFutureApproved1Id);
    public static readonly Guid BundFutureApproved2Guid = Guid.Parse(BundFutureApproved2Id);
    public static readonly Guid BundFutureApprovedKantonStGallen1Guid = Guid.Parse(BundFutureApprovedKantonStGallen1Id);
    public static readonly Guid BundFutureApprovedStadtUzwil1Guid = Guid.Parse(BundFutureApprovedStadtUzwil1Id);
    public static readonly Guid BundFutureApprovedStadtGossau1Guid = Guid.Parse(BundFutureApprovedStadtGossau1Id);
    public static readonly Guid BundFutureApprovedGemeindeArnegg1Guid = Guid.Parse(BundFutureApprovedGemeindeArnegg1Id);
    public static readonly Guid BundFutureApprovedZweckverbandGossauGuid = Guid.Parse(BundFutureApprovedZweckverbandGossauId);
    public static readonly Guid BundFutureApprovedKirchgemeindeArneggGuid = Guid.Parse(BundFutureApprovedKirchgemeindeArneggId);

    public static PoliticalBusiness BundArchivedGemeindeArnegg1 => new()
    {
        Id = BundArchivedGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "V001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote V001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote V001 (official)"),
        Active = false,
        ContestId = ContestMockData.BundArchivedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundArchivedNotApprovedGemeindeArnegg1 => new()
    {
        Id = BundArchivedNotApprovedGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "V001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote V001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote V001 (official)"),
        Active = false,
        ContestId = ContestMockData.BundArchivedNotApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedNotApprovedGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture1 => new()
    {
        Id = BundFuture1Guid,
        PoliticalBusinessNumber = "001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture2 => new()
    {
        Id = BundFuture2Guid,
        PoliticalBusinessNumber = "002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 002 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture3 => new()
    {
        Id = BundFuture3Guid,
        PoliticalBusinessNumber = "003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 003 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture4 => new()
    {
        Id = BundFuture4Guid,
        PoliticalBusinessNumber = "004",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 004 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture5 => new()
    {
        Id = BundFuture5Guid,
        PoliticalBusinessNumber = "005",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 005 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 005 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFuture6 => new()
    {
        Id = BundFuture6Guid,
        PoliticalBusinessNumber = "006",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 006 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 006 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureGemeindeArnegg1 => new()
    {
        Id = BundFutureGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "V001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote V001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote V001 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness SchulgemeindeAndwilArneggFuture1 => new()
    {
        Id = SchulgemeindeAndwilArneggFuture1Guid,
        PoliticalBusinessNumber = "1_1",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future vote 006 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future vote 006 (official)"),
        Active = false,
        ContestId = ContestMockData.SchulgemeindeAndwilArneggFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApproved1 => new()
    {
        Id = BundFutureApproved1Guid,
        PoliticalBusinessNumber = "V001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApproved2 => new()
    {
        Id = BundFutureApproved2Guid,
        PoliticalBusinessNumber = "V002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V002 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedKantonStGallen1 => new()
    {
        Id = BundFutureApprovedKantonStGallen1Guid,
        PoliticalBusinessNumber = "V003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V003 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V003 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedStadtUzwil1 => new()
    {
        Id = BundFutureApprovedStadtUzwil1Guid,
        PoliticalBusinessNumber = "V004",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V004 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedStadtGossau1 => new()
    {
        Id = BundFutureApprovedStadtGossau1Guid,
        PoliticalBusinessNumber = "V005",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V005 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V005 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedGemeindeArnegg1 => new()
    {
        Id = BundFutureApprovedGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "V006",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved vote V006 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved vote V006 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedZweckverbandGossau => new()
    {
        Id = BundFutureApprovedZweckverbandGossauGuid,
        PoliticalBusinessNumber = "VZW",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
        (t, x) => t.ShortDescription = x,
        "bund future approved vote VZW (short)",
        (t, x) => t.OfficialDescription = x,
        "bund future approved vote VZW (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedZweckverbandGossauGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static PoliticalBusiness BundFutureApprovedKirchgemeindeArnegg => new()
    {
        Id = BundFutureApprovedKirchgemeindeArneggGuid,
        PoliticalBusinessNumber = "VKI",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
        (t, x) => t.ShortDescription = x,
        "bund future approved vote VKI (short)",
        (t, x) => t.OfficialDescription = x,
        "bund future approved vote VKI (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKirchgemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.Vote,
    };

    public static IEnumerable<PoliticalBusiness> All
    {
        get
        {
            yield return BundArchivedGemeindeArnegg1;
            yield return BundArchivedNotApprovedGemeindeArnegg1;
            yield return BundFuture1;
            yield return BundFuture2;
            yield return BundFuture3;
            yield return BundFuture4;
            yield return BundFuture5;
            yield return BundFuture6;
            yield return BundFutureGemeindeArnegg1;
            yield return SchulgemeindeAndwilArneggFuture1;
            yield return BundFutureApproved1;
            yield return BundFutureApprovedKantonStGallen1;
            yield return BundFutureApprovedStadtUzwil1;
            yield return BundFutureApproved2;
            yield return BundFutureApprovedStadtGossau1;
            yield return BundFutureApprovedGemeindeArnegg1;
            yield return BundFutureApprovedZweckverbandGossau;
            yield return BundFutureApprovedKirchgemeindeArnegg;
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
