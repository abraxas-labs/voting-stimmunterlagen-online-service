// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class ContestMockData
{
    public const string BundArchivedId = "e2f0b358-e20d-4b46-8d4c-522173d93541";
    public const string BundArchivedNotApprovedId = "59280f22-0b30-4498-97d7-d19d3cae02a2";
    public const string BundFutureApprovedId = "791018cc-6250-46e4-bc80-4a25f00c4d59";
    public const string BundFutureId = "f3393e32-ff61-4007-99f8-51dc7aae52c8";
    public const string BundTodayId = "b776848e-4c4b-4bcc-9522-4b5192765a59";
    public const string BundPastLockedId = "8c3cfd59-3077-4596-bdce-eccf1cca9ac7";
    public const string SchulgemeindeAndwilArneggFutureId = "b8fdae3f-c194-49f3-bc96-5edba59ab3d5";
    public const string PoliticalAssemblyBundFutureApprovedId = "9f6f500e-480f-4459-97d1-db82eebe7b64";

    public static readonly Guid BundArchivedGuid = Guid.Parse(BundArchivedId);
    public static readonly Guid BundArchivedNotApprovedGuid = Guid.Parse(BundArchivedNotApprovedId);
    public static readonly Guid BundFutureApprovedGuid = Guid.Parse(BundFutureApprovedId);
    public static readonly Guid BundFutureGuid = Guid.Parse(BundFutureId);
    public static readonly Guid BundTodayGuid = Guid.Parse(BundTodayId);
    public static readonly Guid BundPastLockedGuid = Guid.Parse(BundPastLockedId);
    public static readonly Guid SchulgemeindeAndwilArneggFutureGuid = Guid.Parse(SchulgemeindeAndwilArneggFutureId);
    public static readonly Guid PoliticalAssemblyBundFutureApprovedGuid = Guid.Parse(PoliticalAssemblyBundFutureApprovedId);

    // this can only be seeded after other seed data,
    // since a lot of builders depend on the contest being in the testing phase
    private static readonly IReadOnlyDictionary<Guid, ContestState> ContestStates = new Dictionary<Guid, ContestState>
        {
            { BundArchivedGuid, ContestState.Archived },
            { BundArchivedNotApprovedGuid, ContestState.Archived },
            { BundTodayGuid, ContestState.Active },
            { BundPastLockedGuid, ContestState.PastLocked },
        };

    // this can only be seeded after other seed data,
    // since this is built by the contest data builder
    // sets the allow manual voter list upload to true
    private static readonly IReadOnlyCollection<Guid> ContestDomainOfInfluencesWithAllowManualVoterListUpload = new[]
    {
        DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        DomainOfInfluenceMockData.ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid,
        DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
        DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
        DomainOfInfluenceMockData.ContestBundArchivedNotApprovedGemeindeArneggGuid,
    };

    public static Contest BundArchived => new()
    {
        Id = BundArchivedGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 000 (BundFuture, archived)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(2),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        Approved = MockedClock.GetDate(-1),
        AttachmentDeliveryDeadline = MockedClock.GetDate(10).Date.NextUtcDate(true),
        PrintingCenterSignUpDeadline = MockedClock.GetDate(5).Date.NextUtcDate(true),
        GenerateVotingCardsDeadline = MockedClock.GetDate(6).Date.NextUtcDate(true),
        EVoting = true,
        OrderNumber = 100000,
    };

    public static Contest BundArchivedNotApproved => new()
    {
        Id = BundArchivedNotApprovedGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 000 (BundFuture, archived, not approved)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(2),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        AttachmentDeliveryDeadline = MockedClock.GetDate(10).Date.NextUtcDate(true),
        PrintingCenterSignUpDeadline = MockedClock.GetDate(5).Date.NextUtcDate(true),
        GenerateVotingCardsDeadline = MockedClock.GetDate(6).Date.NextUtcDate(true),
        EVoting = true,
        OrderNumber = 100001,
    };

    public static Contest BundFutureApproved => new()
    {
        Id = BundFutureApprovedGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 001 (BundFuture, approved)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(2),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        Approved = MockedClock.GetDate(-1),
        AttachmentDeliveryDeadline = MockedClock.GetDate(15).Date.NextUtcDate(true),
        PrintingCenterSignUpDeadline = MockedClock.GetDate(5000).Date.NextUtcDate(true),
        GenerateVotingCardsDeadline = MockedClock.GetDate(5002).Date.NextUtcDate(true),
        EVoting = true,
        OrderNumber = 100002,
    };

    public static Contest BundFuture => new()
    {
        Id = BundFutureGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 002 (BundFuture)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(1),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        EVoting = true,
        OrderNumber = 100003,
    };

    public static Contest BundToday => new()
    {
        Id = BundTodayGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 003 (BundToday)"),
        Date = MockedClock.UtcNowDate.Date,
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        EVoting = true,
        OrderNumber = 100004,
    };

    public static Contest BundPastLocked => new()
    {
        Id = BundPastLockedGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 004(BundPast)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(-1),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        EVoting = true,
        OrderNumber = 100005,
    };

    public static Contest SchulgemeindeAndwilArneggFuture => new()
    {
        Id = SchulgemeindeAndwilArneggFutureGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Contest 005 (Schulgemeinde Andwil Arnegg, Tomorrow)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(1),
        DomainOfInfluenceId = DomainOfInfluenceMockData.SchulgemeindeAndwilArneggGuid,
        IsSingleAttendeeContest = true,
        EVoting = false,
        OrderNumber = 100006,
    };

    public static Contest PoliticalAssemblyBundFutureApproved => new()
    {
        Id = PoliticalAssemblyBundFutureApprovedGuid,
        Translations = TranslationUtil.CreateTranslations<ContestTranslation>(
            (t, x) => t.Description = x,
            "Political assembly 001 (BundFuture)"),
        Date = MockedClock.UtcNowDate.Date.AddDays(1),
        DomainOfInfluenceId = DomainOfInfluenceMockData.BundGuid,
        Approved = MockedClock.GetDate(-1),
        AttachmentDeliveryDeadline = MockedClock.GetDate(15).Date.NextUtcDate(true),
        PrintingCenterSignUpDeadline = MockedClock.GetDate(5000).Date.NextUtcDate(true),
        GenerateVotingCardsDeadline = MockedClock.GetDate(5002).Date.NextUtcDate(true),
        OrderNumber = 100007,
        IsPoliticalAssembly = true,
    };

    public static IEnumerable<Contest> All
    {
        get
        {
            yield return BundArchived;
            yield return BundArchivedNotApproved;
            yield return BundFutureApproved;
            yield return BundFuture;
            yield return BundToday;
            yield return BundPastLocked;
            yield return SchulgemeindeAndwilArneggFuture;
            yield return PoliticalAssemblyBundFutureApproved;
        }
    }

    // Seed data before political businesses
    public static async Task SeedStage1(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        await runScoped(async sp =>
        {
            var contestDataBuilder = sp.GetRequiredService<ContestBuilder>();
            foreach (var item in All)
            {
                await contestDataBuilder.CreateContest(item);
            }
        });
    }

    // Seed data after political businesses
    public static async Task SeedStage2(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        // create steps
        await runScoped(async sp =>
        {
            var builder = sp.GetRequiredService<StepsBuilder>();

            foreach (var contest in All)
            {
                await builder.SyncStepsForContest(contest.Id);
            }
        });

        // set AllowManualVoterListUpload
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var dois = await db.ContestDomainOfInfluences
                .AsTracking()
                .Where(x => ContestDomainOfInfluencesWithAllowManualVoterListUpload.Contains(x.Id))
                .ToListAsync();
            foreach (var doi in dois)
            {
                doi.AllowManualVoterListUpload = true;
            }

            await db.SaveChangesAsync();
        });

        // set testing phase ended
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var stateContestIds = ContestStates.Keys.ToList();
            var contests = await db.Contests.AsTracking()
                .Where(x => stateContestIds.Contains(x.Id))
                .ToListAsync();

            foreach (var contest in contests)
            {
                contest.State = ContestStates[contest.Id];
            }

            await db.SaveChangesAsync();
        });
    }
}
