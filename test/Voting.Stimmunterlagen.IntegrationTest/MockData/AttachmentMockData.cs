// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class AttachmentMockData
{
    public const string BundArchivedGemendeArneggId = "bad9a4b6-f5de-4120-9be6-89c5b861358c";
    public const string BundFutureApprovedGemeindeArneggId = "b4ff2aa7-ff44-483a-b756-476130f9bf2c";
    public const string BundFutureApprovedBund1Id = "ebbc6e0b-bdd9-4962-a76a-4c2767ee9113";
    public const string BundFutureApprovedBund2Id = "4f8bb85c-e120-4cc5-b6a9-8033ee313f35";
    public const string BundFutureApprovedStadtGossauDeliveredId = "bdb7c590-c2b5-49fa-9bc7-1a931a714b97";
    public const string BundFutureApprovedKantonStGallenId = "bfbdd65a-6790-43ca-9d0c-e822557bc674";
    public const string BundFutureApprovedGemeindeArneggWithParentPbsId = "45508c30-14fb-4aa7-9532-db2a79e3c90d";
    public const string PoliticalAssemblyBundFutureApprovedGemeindeArneggId = "c20c3074-1043-441b-9759-dd017f0bb296";

    public static readonly Guid BundArchivedGemeindeArneggGuid = Guid.Parse(BundArchivedGemendeArneggId);
    public static readonly Guid BundFutureApprovedGemeindeArneggGuid = Guid.Parse(BundFutureApprovedGemeindeArneggId);
    public static readonly Guid BundFutureApprovedBund1Guid = Guid.Parse(BundFutureApprovedBund1Id);
    public static readonly Guid BundFutureApprovedBund2Guid = Guid.Parse(BundFutureApprovedBund2Id);
    public static readonly Guid BundFutureApprovedStadtGossauDeliveredGuid = Guid.Parse(BundFutureApprovedStadtGossauDeliveredId);
    public static readonly Guid BundFutureApprovedKantonStGallenGuid = Guid.Parse(BundFutureApprovedKantonStGallenId);
    public static readonly Guid BundFutureApprovedGemeindeArneggWithParentPbsGuid = Guid.Parse(BundFutureApprovedGemeindeArneggWithParentPbsId);
    public static readonly Guid PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid = Guid.Parse(PoliticalAssemblyBundFutureApprovedGemeindeArneggId);

    public static Attachment BundArchivedGemeindeArnegg => new()
    {
        Id = BundArchivedGemeindeArneggGuid,
        Name = "Arnegg Umschlag",
        Category = AttachmentCategory.BallotEnvelopeStandard,
        Format = AttachmentFormat.A5,
        Color = "Blue",
        Supplier = "Kyburz AG",
        DeliveryPlannedOn = MockedClock.GetDate(-10).Date,
        OrderedCount = 2000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = VoteMockData.BundArchivedGemeindeArnegg1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggGuid, RequiredCount = 2000 },
            },
    };

    public static Attachment BundFutureApprovedGemeindeArnegg => new()
    {
        Id = BundFutureApprovedGemeindeArneggGuid,
        Name = "Arnegg Umschlag",
        Category = AttachmentCategory.BallotEnvelopeStandard,
        Format = AttachmentFormat.A5,
        Color = "Blue",
        Supplier = "Kyburz AG",
        DeliveryPlannedOn = MockedClock.GetDate(10).Date,
        OrderedCount = 2500,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = VoteMockData.BundFutureApprovedGemeindeArnegg1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, RequiredCount = 2500 },
            },
    };

    public static Attachment BundFutureApprovedBund1 => new()
    {
        Id = BundFutureApprovedBund1Guid,
        Name = "Bundesbüchlein",
        Category = AttachmentCategory.BrochureCh,
        Format = AttachmentFormat.A4,
        Supplier = "Merkur",
        DeliveryPlannedOn = MockedClock.GetDate(15).Date,
        OrderedCount = 50000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = VoteMockData.BundFutureApproved2Guid },
            },
        Station = 1,
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid, RequiredCount = 0 },
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, RequiredCount = 40 },
            },
    };

    public static Attachment BundFutureApprovedBund2 => new()
    {
        Id = BundFutureApprovedBund2Guid,
        Name = "Bundesbüchlein 2",
        Category = AttachmentCategory.BrochureCh,
        Format = AttachmentFormat.A5,
        Supplier = "Druckverlag",
        DeliveryPlannedOn = MockedClock.GetDate(30).Date,
        OrderedCount = 50000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
                new() { PoliticalBusinessId = ProportionalElectionMockData.BundFutureApproved1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, RequiredCount = 40 },
            },
    };

    public static Attachment BundFutureApprovedStadtGossauDelivered => new()
    {
        Id = BundFutureApprovedStadtGossauDeliveredGuid,
        Name = "Gossau Umschlag",
        Category = AttachmentCategory.BallotEnvelopeStandard,
        Format = AttachmentFormat.A5,
        Supplier = "kdmz",
        DeliveryPlannedOn = MockedClock.GetDate(20).Date,
        OrderedCount = 7000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
        State = AttachmentState.Delivered,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = MajorityElectionMockData.BundFutureApprovedStadtGossau1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid, RequiredCount = 7000 },
            },
    };

    public static Attachment BundFutureApprovedKantonStGallen => new()
    {
        Id = BundFutureApprovedKantonStGallenGuid,
        Name = "St- Gallen Umschlag",
        Category = AttachmentCategory.BallotEnvelopeStandard,
        Format = AttachmentFormat.A4,
        Supplier = "kdmz",
        DeliveryPlannedOn = MockedClock.GetDate(15).Date,
        OrderedCount = 20000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new PoliticalBusinessAttachmentEntry { PoliticalBusinessId = MajorityElectionMockData.BundFutureApprovedKantonStGallen1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedKantonStGallenGuid, RequiredCount = 0 },
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, RequiredCount = 100 },
            },
    };

    public static Attachment BundFutureApprovedGemeindeArneggWithParentPbs => new()
    {
        Id = BundFutureApprovedGemeindeArneggWithParentPbsGuid,
        Name = "Spezielles Arnegg Büchlein",
        Category = AttachmentCategory.OtherMu,
        Format = AttachmentFormat.A6,
        Supplier = "Städtische Druckerei",
        DeliveryPlannedOn = MockedClock.GetDate(18).Date,
        OrderedCount = 1000,
        DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        PoliticalBusinessEntries = new List<PoliticalBusinessAttachmentEntry>
            {
                new() { PoliticalBusinessId = VoteMockData.BundFutureApproved1Guid },
            },
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid, RequiredCount = 1000 },
            },
    };

    public static Attachment PoliticalAssemblyBundFutureApprovedGemeindeArnegg => new()
    {
        Id = PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
        Name = "Arnegg Umschlag",
        Category = AttachmentCategory.BallotEnvelopeStandard,
        Format = AttachmentFormat.A5,
        Color = "Blue",
        Supplier = "Kyburz AG",
        DeliveryPlannedOn = MockedClock.GetDate(10).Date,
        OrderedCount = 2500,
        DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid,
        DomainOfInfluenceAttachmentCounts = new List<DomainOfInfluenceAttachmentCount>
            {
                new() { DomainOfInfluenceId = DomainOfInfluenceMockData.PoliticalAssemblyBundFutureApprovedGemeindeArneggGuid, RequiredCount = 2500 },
            },
    };

    public static IEnumerable<Attachment> All
    {
        get
        {
            yield return BundArchivedGemeindeArnegg;
            yield return BundFutureApprovedGemeindeArnegg;
            yield return BundFutureApprovedBund1;
            yield return BundFutureApprovedBund2;
            yield return BundFutureApprovedStadtGossauDelivered;
            yield return BundFutureApprovedKantonStGallen;
            yield return BundFutureApprovedGemeindeArneggWithParentPbs;
            yield return PoliticalAssemblyBundFutureApprovedGemeindeArnegg;
        }
    }

    public static Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        var all = All.ToList();
        return runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();

            var doiHierarchies = await db.ContestDomainOfInfluenceHierarchyEntries
                .Where(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!.Any(y => y.Role == PoliticalBusinessRole.Attendee))
                .ToListAsync();

            foreach (var attachment in all)
            {
                var doiAttachmentCounts = attachment.DomainOfInfluenceAttachmentCounts?.ToList() ?? new();
                var existingDoiIds = doiAttachmentCounts.ConvertAll(c => c.DomainOfInfluenceId);
                var doiIds = doiHierarchies
                    .Where(x => x.ParentDomainOfInfluenceId == attachment.DomainOfInfluenceId)
                    .Select(x => x.DomainOfInfluenceId)
                    .ToList();

                doiIds.Add(attachment.DomainOfInfluenceId);

                foreach (var doiId in doiIds)
                {
                    if (existingDoiIds.Contains(doiId))
                    {
                        continue;
                    }

                    doiAttachmentCounts.Add(new()
                    {
                        DomainOfInfluenceId = doiId,
                    });
                }

                attachment.DomainOfInfluenceAttachmentCounts = doiAttachmentCounts;
            }

            db.Attachments.AddRange(all);
            await db.SaveChangesAsync();
        });
    }
}
