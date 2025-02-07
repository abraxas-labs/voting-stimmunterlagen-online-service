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

public static class DomainOfInfluenceMockData
{
    public const string BundId = "d59da8b8-8af3-4082-afe1-db133bc21897";
    public const string KantonStGallenId = "8acf818a-45da-4738-a546-4c8428a6a6b1";
    public const string StadtStGallenId = "d0d4b2b6-61ac-4c03-9f1d-328b9de82d01";
    public const string StadtGossauId = "47ef9998-8361-474b-a3f5-04961d28db1b";
    public const string StadtUzwilId = "f94a4ffc-3949-4e94-bbbb-1c4d2621983f";
    public const string SchulgemeindeAndwilArneggId = "bb3f6e2f-d8a1-4cb5-9511-fb3e2c53c87e";
    public const string GemeindeArneggId = "f2f20f24-fd80-40c1-9307-8dab12f102bb";
    public const string AuslandschweizerId = "747b552e-88ad-46be-a472-709c4b7ba996";

    public static readonly Guid BundGuid = Guid.Parse(BundId);
    public static readonly Guid KantonStGallenGuid = Guid.Parse(KantonStGallenId);
    public static readonly Guid StadtStGallenGuid = Guid.Parse(StadtStGallenId);
    public static readonly Guid StadtGossauGuid = Guid.Parse(StadtGossauId);
    public static readonly Guid StadtUzwilGuid = Guid.Parse(StadtUzwilId);
    public static readonly Guid SchulgemeindeAndwilArneggGuid = Guid.Parse(SchulgemeindeAndwilArneggId);
    public static readonly Guid GemeindeArneggGuid = Guid.Parse(GemeindeArneggId);
    public static readonly Guid AuslandschweizerGuid = Guid.Parse(AuslandschweizerId);

    public static readonly Guid ContestBundArchivedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundArchivedGuid, GemeindeArneggGuid);
    public static readonly Guid ContestBundArchivedNotApprovedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundArchivedNotApprovedGuid, GemeindeArneggGuid);

    public static readonly Guid ContestBundFutureApprovedBundGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, BundGuid);
    public static readonly Guid ContestBundFutureApprovedKantonStGallenGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, KantonStGallenGuid);
    public static readonly Guid ContestBundFutureApprovedStadtGossauGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, StadtGossauGuid);
    public static readonly Guid ContestBundFutureApprovedStadtUzwilGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, StadtUzwilGuid);
    public static readonly Guid ContestBundFutureApprovedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, GemeindeArneggGuid);
    public static readonly Guid ContestBundFutureApprovedAuslandschweizerGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureApprovedGuid, AuslandschweizerGuid);

    public static readonly Guid ContestBundFutureBundGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, BundGuid);
    public static readonly Guid ContestBundFutureKantonStGallenGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, KantonStGallenGuid);
    public static readonly Guid ContestBundFutureStadtGossauGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, StadtGossauGuid);
    public static readonly Guid ContestBundFutureStadtUzwilGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, StadtUzwilGuid);
    public static readonly Guid ContestBundFutureGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, GemeindeArneggGuid);
    public static readonly Guid ContestBundFutureSchulgemeindeAndwilArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.BundFutureGuid, SchulgemeindeAndwilArneggGuid);

    public static readonly Guid ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.SchulgemeindeAndwilArneggFutureGuid, SchulgemeindeAndwilArneggGuid);

    public static readonly Guid PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ContestMockData.PoliticalAssemblyBundFutureApprovedGuid, GemeindeArneggGuid);

    public static readonly string ContestBundArchivedGemeindeArneggId = ContestBundArchivedGemeindeArneggGuid.ToString();
    public static readonly string ContestBundArchivedNotApprovedGemeindeArneggId = ContestBundArchivedNotApprovedGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureApprovedBundId = ContestBundFutureApprovedBundGuid.ToString();
    public static readonly string ContestBundFutureApprovedKantonStGallenId = ContestBundFutureApprovedKantonStGallenGuid.ToString();
    public static readonly string ContestBundFutureApprovedStadtGossauId = ContestBundFutureApprovedStadtGossauGuid.ToString();
    public static readonly string ContestBundFutureApprovedStadtUzwilId = ContestBundFutureApprovedStadtUzwilGuid.ToString();
    public static readonly string ContestBundFutureApprovedGemeindeArneggId = ContestBundFutureApprovedGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureBundId = ContestBundFutureBundGuid.ToString();
    public static readonly string ContestBundFutureKantonStGallenId = ContestBundFutureKantonStGallenGuid.ToString();
    public static readonly string ContestBundFutureStadtGossauId = ContestBundFutureStadtGossauGuid.ToString();
    public static readonly string ContestBundFutureStadtUzwilId = ContestBundFutureStadtUzwilGuid.ToString();
    public static readonly string ContestBundFutureGemeindeArneggId = ContestBundFutureGemeindeArneggGuid.ToString();

    public static readonly string ContestBundFutureSchulgemeindeAndwilArneggId = ContestBundFutureSchulgemeindeAndwilArneggGuid.ToString();

    public static readonly string ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggId = ContestSchulgemeindeAndwilArneggFutureSchulgemeindeAndwilArneggGuid.ToString();

    public static readonly string PoliticalAssemblyBundFutureApprovedGemeindeArneggId = PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid.ToString();

    public static DomainOfInfluence Bund => new()
    {
        Id = BundGuid,
        RootId = BundGuid,
        Name = "Bund",
        AuthorityName = "Abraxas Informatik AG",
        ShortName = "CH",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.Abraxas,
        Type = DomainOfInfluenceType.Ch,
        Bfs = "1000",
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
    };

    public static DomainOfInfluence KantonStGallen => new()
    {
        Id = KantonStGallenGuid,
        Name = "Kanton St. Gallen",
        AuthorityName = "Staatskanzlei St. Gallen",
        ShortName = "KT-SG",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.StaatskanzleiStGallen,
        ParentId = BundGuid,
        RootId = BundGuid,
        Type = DomainOfInfluenceType.Ct,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "1100",
    };

    public static DomainOfInfluence StadtStGallen => new()
    {
        Id = StadtStGallenGuid,
        Name = "Stadt St. Gallen",
        AuthorityName = "Stadt St. Gallen",
        ShortName = "MU-SG",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.StaatskanzleiStGallen,
        ParentId = KantonStGallenGuid,
        RootId = BundGuid,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Mu,
        Bfs = "1110",
        LogoRef = "StGallen_Logo.png",
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        SapCustomerOrderNumber = "00073000",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.A,
            ShippingReturn = VotingCardShippingFranking.B2,
            ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "111994573",
            FrankingLicenceReturnNumber = "119302122",
        },
        ReturnAddress = new()
        {
            AddressAddition = "o.V.i.A",
            AddressLine1 = "Parlamentsdienste",
            AddressLine2 = "EG",
            Street = "Regierungsgebäude",
            ZipCode = "9001",
            City = "St. Gallen",
            Country = "Schweiz",
        },
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.StadtStGallenGuid,
            },
        },
    };

    public static DomainOfInfluence StadtGossau => new()
    {
        Id = StadtGossauGuid,
        Name = "Stadt Gossau",
        AuthorityName = "Stadt St. Gossau",
        ShortName = "MU-GO",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.StadtGossau,
        ParentId = KantonStGallenGuid,
        RootId = BundGuid,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Mu,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "1120",
        LogoRef = "gossau-logo.png",
        SapCustomerOrderNumber = "00073100",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.A,
            ShippingReturn = VotingCardShippingFranking.WithoutFranking,
            ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "505964478",
            FrankingLicenceReturnNumber = "994412387",
        },
        ReturnAddress = new()
        {
            AddressLine1 = "Stadtverwaltung Gossau",
            Street = "Bahnhofstrasse 25",
            ZipCode = "9200",
            City = "Gossau",
            Country = "Schweiz",
        },
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.StadtGossauGuid,
            },
        },
        StistatMunicipality = true,
    };

    public static DomainOfInfluence StadtUzwil => new()
    {
        Id = StadtUzwilGuid,
        Name = "Stadt Uzwil",
        AuthorityName = "Stadt St. Uzwil",
        ShortName = "MU-UZ",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.StadtUzwil,
        ParentId = KantonStGallenGuid,
        RootId = BundGuid,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Mu,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "1130",
        LogoRef = "uzwil_logo.png",
        SapCustomerOrderNumber = "00093109",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.B1,
            ShippingReturn = VotingCardShippingFranking.GasA,
            ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToMunicipality,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "222994573",
            FrankingLicenceReturnNumber = "229302122",
        },
        ReturnAddress = new()
        {
            AddressLine1 = "Gemeinderat Uzwil",
            AddressLine2 = "2. Stockwerk",
            Street = "Stickereiplatz 1",
            ZipCode = "9240",
            City = "Uzwil",
            Country = "Schweiz",
        },
        ExternalPrintingCenter = true,
        ExternalPrintingCenterEaiMessageType = "EXT-Uzwil",
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.StadtUzwilGuid,
            },
        },
    };

    public static DomainOfInfluence GemeindeArnegg => new()
    {
        Id = GemeindeArneggGuid,
        Name = "Gemeinde Arnegg",
        AuthorityName = "Gemeinde Arnegg",
        ShortName = "MU-AG",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg,
        ParentId = KantonStGallenGuid,
        RootId = BundGuid,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Mu,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "1240",
        LogoRef = "arnegg_logo.png",
        SapCustomerOrderNumber = "00073101",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.A,
            ShippingReturn = VotingCardShippingFranking.A,
            ShippingMethod = VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "333994573",
            FrankingLicenceReturnNumber = "339302122",
        },
        ReturnAddress = new()
        {
            AddressLine1 = "Gemeinde Andwil-Arnegg",
            Street = "Lätschenstrasse 7",
            ZipCode = "9204",
            City = "Andwil",
            Country = "Schweiz",
        },
        VotingCardColor = VotingCardColor.Green,
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.GemeindeArneggGuid,
            },
        },
        StistatMunicipality = true,
    };

    public static DomainOfInfluence SchulgemeindeArneggAndwil => new()
    {
        Id = SchulgemeindeAndwilArneggGuid,
        RootId = SchulgemeindeAndwilArneggGuid,
        Name = "Schulgemeinde Andwil Arnegg",
        AuthorityName = "Gemeinde Arnegg",
        ShortName = "SU-AA",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Sc,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "2000",
        LogoRef = "arnegg_logo.png",
        SapCustomerOrderNumber = "00073102",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.B1,
            ShippingReturn = VotingCardShippingFranking.A,
            ShippingMethod = VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "444994573",
            FrankingLicenceReturnNumber = "449302122",
        },
        ReturnAddress = new()
        {
            AddressLine1 = "Gemeinde Andwil-Arnegg",
            Street = "Lätschenstrasse 7",
            ZipCode = "9204",
            City = "Andwil",
            Country = "Schweiz",
        },
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.SchulgemeindeAndwilArneggGuid,
            },
        },
        VotingCardFlatRateDisabled = true,
    };

    public static DomainOfInfluence Auslandschweizer => new()
    {
        Id = AuslandschweizerGuid,
        Name = "Auslandschweizer",
        AuthorityName = "Auslandschweizer",
        ShortName = "MU-AS",
        SecureConnectId = MockDataSeeder.SecureConnectTenantIds.Auslandschweizer,
        ParentId = KantonStGallenGuid,
        RootId = BundGuid,
        ResponsibleForVotingCards = true,
        Type = DomainOfInfluenceType.Mu,
        Canton = DomainOfInfluenceCanton.Sg,
        CantonDefaults = new DomainOfInfluenceCantonDefaults
        {
            VotingDocumentsEVotingEaiMessageType = "EVOT-SG",
        },
        ElectoralRegistrationEnabled = true,
        Bfs = "9170",
        LogoRef = "auslandschweizer_logo.png",
        SapCustomerOrderNumber = "00099999",
        PrintData = new()
        {
            ShippingAway = VotingCardShippingFranking.A,
            ShippingReturn = VotingCardShippingFranking.A,
            ShippingMethod = VotingCardShippingMethod.OnlyPrintingPackagingToMunicipality,
        },
        SwissPostData = new()
        {
            InvoiceReferenceNumber = "555994573",
            FrankingLicenceReturnNumber = "559302122",
        },
        ReturnAddress = new()
        {
            AddressLine1 = "Auslandschweizer",
            Street = "Seidenstrasse",
            ZipCode = "1000",
            City = "Stadt",
            Country = "Ausland",
        },
        CountingCircles = new List<DomainOfInfluenceCountingCircle>
        {
            new()
            {
                CountingCircleId = CountingCircleMockData.AuslandschweizerGuid,
            },
        },
    };

    public static IEnumerable<DomainOfInfluence> All
    {
        get
        {
            yield return Bund;
            yield return KantonStGallen;
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
            db.DomainOfInfluences.AddRange(all);
            AddHierarchyEntries(all, db);
            await db.SaveChangesAsync();
        });
    }

    private static void AddHierarchyEntries(List<DomainOfInfluence> dois, DataContext db)
    {
        var byId = dois.ToDictionary(x => x.Id);
        foreach (var doi in dois)
        {
            var currentDoi = doi;
            while (currentDoi.ParentId.HasValue)
            {
                db.DomainOfInfluenceHierarchyEntries.Add(new DomainOfInfluenceHierarchyEntry
                {
                    DomainOfInfluenceId = doi.Id,
                    ParentDomainOfInfluenceId = currentDoi.ParentId.Value,
                });

                currentDoi = byId[currentDoi.ParentId.Value];
            }
        }
    }
}
