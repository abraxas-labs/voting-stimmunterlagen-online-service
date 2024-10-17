// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class CountingCircleMockData
{
    public const string StadtStGallenId = "29240676-b230-4daf-a6d6-48a6151a12f5";
    public const string StadtGossauId = "c5264dab-bf1e-4425-a169-92af5f166413";
    public const string StadtUzwilId = "50068648-d085-4546-a0d7-01cc25933bbb";
    public const string SchulgemeindeAndwilArneggId = "044d573c-0460-4e94-bd34-73e923337b3c";
    public const string GemeindeArneggId = "39d5a7af-54a8-49f3-9385-3b7911ef9bc6";
    public const string AuslandschweizerId = "69284b46-24bf-4fd9-bce1-b7f0832e552c";

    public static readonly Guid StadtStGallenGuid = Guid.Parse(StadtStGallenId);
    public static readonly Guid StadtGossauGuid = Guid.Parse(StadtGossauId);
    public static readonly Guid StadtUzwilGuid = Guid.Parse(StadtUzwilId);
    public static readonly Guid SchulgemeindeAndwilArneggGuid = Guid.Parse(SchulgemeindeAndwilArneggId);
    public static readonly Guid GemeindeArneggGuid = Guid.Parse(GemeindeArneggId);
    public static readonly Guid AuslandschweizerGuid = Guid.Parse(AuslandschweizerId);

    public static readonly Guid ContestBundArchivedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundArchivedGuid, GemeindeArneggGuid);
    public static readonly Guid ContestBundArchivedNotApprovedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundArchivedNotApprovedGuid, GemeindeArneggGuid);

    public static readonly Guid ContestBundFutureApprovedStadtGossauGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureApprovedGuid, StadtGossauGuid);
    public static readonly Guid ContestBundFutureApprovedStadtUzwilGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureApprovedGuid, StadtUzwilGuid);
    public static readonly Guid ContestBundFutureApprovedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureApprovedGuid, GemeindeArneggGuid);

    public static readonly Guid ContestBundFutureStadtGossauGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureGuid, StadtGossauGuid);
    public static readonly Guid ContestBundFutureStadtUzwilGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureGuid, StadtUzwilGuid);
    public static readonly Guid ContestBundFutureGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureGuid, GemeindeArneggGuid);
    public static readonly Guid ContestBundFutureSchulgemeindeAndwilArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.BundFutureGuid, SchulgemeindeAndwilArneggGuid);

    public static readonly Guid ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid = StimmunterlagenUuidV5.BuildContestCountingCircle(ContestMockData.SchulgemeindeAndwilArneggFutureGuid, SchulgemeindeAndwilArneggGuid);

    public static readonly string ContestBundArchivedGemeindeArneggId = ContestBundArchivedGemeindeArneggGuid.ToString();
    public static readonly string ContestBundArchivedNotApprovedGemeindeArneggId = ContestBundArchivedNotApprovedGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureApprovedStadtGossauId = ContestBundFutureApprovedStadtGossauGuid.ToString();
    public static readonly string ContestBundFutureApprovedStadtUzwilId = ContestBundFutureApprovedStadtUzwilGuid.ToString();
    public static readonly string ContestBundFutureApprovedGemeindeArneggId = ContestBundFutureApprovedGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureStadtGossauId = ContestBundFutureStadtGossauGuid.ToString();
    public static readonly string ContestBundFutureStadtUzwilId = ContestBundFutureStadtUzwilGuid.ToString();
    public static readonly string ContestBundFutureGemeindeArneggId = ContestBundFutureGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureSchulgemeindeAndwilArneggId = ContestBundFutureSchulgemeindeAndwilArneggGuid.ToString();

    public static readonly string ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggId = ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid.ToString();

    public static CountingCircle StadtStGallen => new()
    {
        Id = StadtStGallenGuid,
        Name = "Stadt St. Gallen",
        Bfs = "1110",
    };

    public static CountingCircle StadtGossau => new()
    {
        Id = StadtGossauGuid,
        Name = "Stadt Gossau",
        Bfs = "1120",
    };

    public static CountingCircle StadtUzwil => new()
    {
        Id = StadtUzwilGuid,
        Name = "Stadt Uzwil",
        Bfs = "1130",
    };

    public static CountingCircle GemeindeArnegg => new()
    {
        Id = GemeindeArneggGuid,
        Name = "Gemeinde Arnegg",
        Bfs = "1240",
    };

    public static CountingCircle SchulgemeindeArneggAndwil => new()
    {
        Id = SchulgemeindeAndwilArneggGuid,
        Name = "Schulgemeinde Andwil Arnegg",
        Bfs = "2000",
    };

    public static CountingCircle Auslandschweizer => new()
    {
        Id = AuslandschweizerGuid,
        Name = "Auslandschweizer",
        Bfs = "9170",
    };

    public static IEnumerable<CountingCircle> All
    {
        get
        {
            yield return StadtStGallen;
            yield return StadtGossau;
            yield return StadtUzwil;
            yield return GemeindeArnegg;
            yield return SchulgemeindeArneggAndwil;
            yield return Auslandschweizer;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        var all = All.ToList();
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            db.CountingCircles.AddRange(all);
            await db.SaveChangesAsync();
        });
    }
}
