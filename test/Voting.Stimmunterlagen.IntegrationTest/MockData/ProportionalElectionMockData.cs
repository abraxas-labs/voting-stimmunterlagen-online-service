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

public static class ProportionalElectionMockData
{
    public const string BundFuture1Id = "0ec585d0-0722-4129-9184-0b3deaf6c2ef";
    public const string BundFuture2Id = "316f4fc7-e031-45b5-b717-0272636c9c22";
    public const string BundFuture3Id = "fe50110e-d374-460a-a108-dd0c1928545c";
    public const string BundFuture4Id = "07139842-7531-489d-984b-afab8d69810b";
    public const string BundFutureGemeindeArnegg1Id = "1c22116b-c97b-4983-972a-623770799fc7";
    public const string BundFutureStadtUzwil1Id = "d2ec0817-0373-46bb-bfaa-68ec9c63d0ae";
    public const string BundFutureApproved1Id = "8c3f98aa-46ec-4b7a-9c99-c557f40ae592";
    public const string BundFutureApprovedStadtUzwil1Id = "8a0ff955-e6d5-4a54-bd37-cd6ecbf8e475";
    public const string BundFutureApprovedGemeindeArnegg1Id = "2770e4c7-fc2c-4600-83df-c9215dc20cb1";

    public static readonly Guid BundFuture1Guid = Guid.Parse(BundFuture1Id);
    public static readonly Guid BundFuture2Guid = Guid.Parse(BundFuture2Id);
    public static readonly Guid BundFuture3Guid = Guid.Parse(BundFuture3Id);
    public static readonly Guid BundFuture4Guid = Guid.Parse(BundFuture4Id);
    public static readonly Guid BundFutureGemeindeArnegg1Guid = Guid.Parse(BundFutureGemeindeArnegg1Id);
    public static readonly Guid BundFutureUzwil1Guid = Guid.Parse(BundFutureStadtUzwil1Id);
    public static readonly Guid BundFutureApproved1Guid = Guid.Parse(BundFutureApproved1Id);
    public static readonly Guid BundFutureApprovedUzwil1Guid = Guid.Parse(BundFutureApprovedStadtUzwil1Id);
    public static readonly Guid BundFutureApprovedGemeindeArnegg1Guid = Guid.Parse(BundFutureApprovedGemeindeArnegg1Id);

    public static PoliticalBusiness BundFuture1 => new()
    {
        Id = BundFuture1Guid,
        PoliticalBusinessNumber = "001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection 001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection 001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture2 => new()
    {
        Id = BundFuture2Guid,
        PoliticalBusinessNumber = "002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection 002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection 002 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureKantonStGallenGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture3 => new()
    {
        Id = BundFuture3Guid,
        PoliticalBusinessNumber = "003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection 003 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFuture4 => new()
    {
        Id = BundFuture4Guid,
        PoliticalBusinessNumber = "004",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection 004 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection 004 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureGemeindeArnegg1 => new()
    {
        Id = BundFutureGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "P001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection P001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection P001 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureUzwil1 => new()
    {
        Id = BundFutureUzwil1Guid,
        PoliticalBusinessNumber = "P002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future ProportionalElection P002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future ProportionalElection P002 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApproved1 => new()
    {
        Id = BundFutureApproved1Guid,
        PoliticalBusinessNumber = "P001",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved ProportionalElection P001 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved ProportionalElection P001 (official)"),
        Active = true,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApprovedUzwil1 => new()
    {
        Id = BundFutureApprovedUzwil1Guid,
        PoliticalBusinessNumber = "P002",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved ProportionalElection P002 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved ProportionalElection P002 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
        Approved = true,
    };

    public static PoliticalBusiness BundFutureApprovedGemeindeArnegg1 => new()
    {
        Id = BundFutureApprovedGemeindeArnegg1Guid,
        PoliticalBusinessNumber = "P003",
        Translations = TranslationUtil.CreateTranslations<PoliticalBusinessTranslation>(
            (t, x) => t.ShortDescription = x,
            "bund future approved ProportionalElection P003 (short)",
            (t, x) => t.OfficialDescription = x,
            "bund future approved ProportionalElection P003 (official)"),
        Active = false,
        ContestId = ContestMockData.BundFutureApprovedGuid,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        PoliticalBusinessType = PoliticalBusinessType.ProportionalElection,
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
            yield return BundFutureGemeindeArnegg1;
            yield return BundFutureUzwil1;
            yield return BundFutureApproved1;
            yield return BundFutureApprovedUzwil1;
            yield return BundFutureApprovedGemeindeArnegg1;
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
