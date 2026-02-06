// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using Abraxas.Voting.Basis.Shared.V1;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Snapper;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;
using Attachment = Voting.Stimmunterlagen.Data.Models.Attachment;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;
using VotingCardShippingFranking = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingFranking;
using VotingCardShippingMethod = Abraxas.Voting.Basis.Shared.V1.VotingCardShippingMethod;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class DomainOfInfluenceProcessorTest : BaseWriteableDbTest
{
    public DomainOfInfluenceProcessorTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DomainOfInfluenceCreated()
    {
        var id = Guid.Parse("f46f1e84-cbcb-40a6-bf40-b4dce9782a81");
        var secureConnectId = "wk-sg-001-new";
        var eventData = new DomainOfInfluenceCreated
        {
            DomainOfInfluence = new DomainOfInfluenceEventData
            {
                Id = id.ToString(),
                Name = "St.Gallen Tablat (neu)",
                AuthorityName = "Staatskanzlei St. Gallen",
                ShortName = "WK SG",
                SecureConnectId = secureConnectId,
                ParentId = DomainOfInfluenceMockData.KantonStGallenId,
                ResponsibleForVotingCards = true,
                ElectoralRegistrationEnabled = true,
                ElectoralRegisterMultipleEnabled = true,
            },
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate doi itself
        var doi = await RunOnDb(db => db.DomainOfInfluences.FirstAsync(x => x.Id == id));
        doi.ShouldMatchChildSnapshot("doi");

        // validate created in contests
        var contestsInTestPhase = await RunOnDb(db => db.Contests.WhereInTestingPhase().CountAsync());
        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences
                .Where(x => x.BasisDomainOfInfluenceId == id)
                .OrderBy(x => x.ContestId)
                .Select(x => new { x.ContestId, x.BasisDomainOfInfluenceId, x.Role, x.Name, x.Canton, x.CantonDefaults.VotingDocumentsEVotingEaiMessageType })
                .ToListAsync());
        contestDois.Should().HaveCount(contestsInTestPhase);
        contestDois.ShouldMatchChildSnapshot("contest_doi");

        // validate assigned contest counting circles
        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");

        // validate doi hierarchy
        var hierarchyEntries = await RunOnDb(db => db.DomainOfInfluenceHierarchyEntries
            .Where(x => x.DomainOfInfluenceId == id)
            .OrderBy(x => x.ParentDomainOfInfluenceId)
            .Select(x => new
            {
                x.DomainOfInfluence!.Name,
                ParentName = x.ParentDomainOfInfluence!.Name,
                x.ParentDomainOfInfluenceId,
            })
            .ToListAsync());
        hierarchyEntries.ShouldMatchChildSnapshot("doi_hierarchy");

        // validate doi hierarchy in contests
        var contestHierarchyEntries = await RunOnDb(db => db.ContestDomainOfInfluenceHierarchyEntries
                .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
                .OrderBy(x => x.DomainOfInfluence!.ContestId)
                .ThenBy(x => x.DomainOfInfluence!.Name)
                .ThenBy(x => x.ParentDomainOfInfluence!.Name)
                .Select(x => new
                {
                    Contest = x.DomainOfInfluence!.Contest!.Translations!.OrderBy(x => x.Language).First().Description,
                    x.DomainOfInfluence!.BasisDomainOfInfluenceId,
                    x.DomainOfInfluence.Name,
                    ParentName = x.ParentDomainOfInfluence!.Name,
                })
                .ToListAsync());
        contestHierarchyEntries.ShouldMatchChildSnapshot("doi_hierarchy_contest");

        // validate steps
        var steps = await RunOnDb(db => db.StepStates
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.DomainOfInfluence!.ContestId)
            .ThenBy(x => x.Step)
            .Select(x => new { x.Step, x.DomainOfInfluence!.ContestId, x.DomainOfInfluence!.BasisDomainOfInfluenceId, x.DomainOfInfluence!.Name })
            .ToListAsync());
        steps.ShouldMatchChildSnapshot("steps");

        // validate sra layouts
        var doiLayouts = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.DomainOfInfluence!.ContestId)
            .ThenBy(x => x.VotingCardType)
            .Select(x => new { x.DomainOfInfluence!.Name, x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        doiLayouts.ShouldMatchChildSnapshot("doi-layouts");

        // publish another event to see if contest data is updated correctly
        await TestEventPublisher.Publish(2, new DomainOfInfluenceCreated
        {
            DomainOfInfluence = new DomainOfInfluenceEventData
            {
                Id = id.ToString(),
                Name = "St.Gallen Tablat (neu) 2",
                AuthorityName = "Staatskanzlei St. Gallen 2",
                ShortName = "WK SG",
                SecureConnectId = secureConnectId,
                ParentId = DomainOfInfluenceMockData.KantonStGallenId,
                ResponsibleForVotingCards = true,
            },
        });
        (await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Id == id)))
            .Name
            .Should()
            .Be("St.Gallen Tablat (neu) 2");
        var updatedContestDoiNames = await RunOnDb(db => db.ContestDomainOfInfluences
            .Include(x => x.Contest)
            .Where(x => x.BasisDomainOfInfluenceId == id)
            .Select(x => x.Name)
            .ToListAsync());
        updatedContestDoiNames.Should().HaveCount(4);
        updatedContestDoiNames.Distinct().Single().Should().Be("St.Gallen Tablat (neu) 2");
    }

    [Fact]
    public async Task DomainOfInfluenceUpdated()
    {
        var secureConnectId = "SC-MU-GO-Updated";
        var originalName = DomainOfInfluenceMockData.StadtGossau.Name;
        var name = "Gossau updated";
        var id = DomainOfInfluenceMockData.StadtGossauGuid;
        var eventData = new DomainOfInfluenceUpdated
        {
            DomainOfInfluence = new DomainOfInfluenceEventData
            {
                Id = id.ToString(),
                Name = name,
                AuthorityName = "Gossau",
                ShortName = "MU GO",
                SecureConnectId = secureConnectId,
                ParentId = DomainOfInfluenceMockData.KantonStGallenId,
                ResponsibleForVotingCards = true,
                ElectoralRegistrationEnabled = true,
                ElectoralRegisterMultipleEnabled = true,
            },
        };

        // remove data to test the sync
        await RunOnDb(async db =>
        {
            var doiAttachmentCount = await db.DomainOfInfluenceAttachmentCounts
                .SingleAsync(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid && x.AttachmentId == AttachmentMockData.BundFutureApprovedBund2Guid);

            db.DomainOfInfluenceAttachmentCounts.Remove(doiAttachmentCount);
            await db.SaveChangesAsync();
        });

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate doi itself
        var doi = await RunOnDb(db => db.DomainOfInfluences.FirstAsync(x => x.Id == id));
        doi.ShouldMatchChildSnapshot("doi");

        // validate assigned counting circles
        var doiCcs = await RunOnDb(db => db.DomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluenceId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        doiCcs.ShouldMatchChildSnapshot("doi_ccs");

        // validate updated in contests
        var contestsInTestPhase = await RunOnDb(db => db.Contests.WhereInTestingPhase().CountAsync());
        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.ContestId)
            .Select(x => new { x.ContestId, x.BasisDomainOfInfluenceId, x.Role, x.Name, x.Canton, x.CantonDefaults.VotingDocumentsEVotingEaiMessageType, x.PrintData!.ShippingAway, x.ReturnAddress!.AddressLine1 })
            .ToListAsync());
        contestDois.Count(x => x.Name == name).Should().Be(contestsInTestPhase);
        contestDois.Count(x => x.Name == originalName).Should().Be(4);
        contestDois.ShouldMatchChildSnapshot("contest_doi");

        // validate doi hierarchy
        var hierarchyEntries = await RunOnDb(db => db.DomainOfInfluenceHierarchyEntries
            .Where(x => x.DomainOfInfluenceId == id)
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ThenBy(x => x.ParentDomainOfInfluence!.Name)
            .Select(x => new
            {
                x.DomainOfInfluence!.Name,
                ParentName = x.ParentDomainOfInfluence!.Name,
                x.ParentDomainOfInfluenceId,
            })
            .ToListAsync());
        hierarchyEntries.ShouldMatchChildSnapshot("doi_hierarchy");

        // validate doi hierarchy in contests
        var contestHierarchyEntries = await RunOnDb(db => db.ContestDomainOfInfluenceHierarchyEntries
                .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
                .OrderBy(x => x.DomainOfInfluence!.ContestId)
                .ThenBy(x => x.DomainOfInfluence!.Name)
                .ThenBy(x => x.ParentDomainOfInfluence!.Name)
                .Select(x => new
                {
                    Contest = x.DomainOfInfluence!.Contest!.Translations!.OrderBy(x => x.Language).First().Description,
                    x.DomainOfInfluence!.BasisDomainOfInfluenceId,
                    x.DomainOfInfluence.Name,
                    ParentName = x.ParentDomainOfInfluence!.Name,
                })
                .ToListAsync());
        contestHierarchyEntries.ShouldMatchChildSnapshot("doi_hierarchy_contest");

        // validate steps
        var steps = await RunOnDb(db => db.StepStates
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.DomainOfInfluence!.ContestId)
            .ThenBy(x => x.Step)
            .Select(x => new { x.Step, x.DomainOfInfluence!.ContestId, x.DomainOfInfluence!.BasisDomainOfInfluenceId, x.DomainOfInfluence!.Name })
            .ToListAsync());
        steps.ShouldMatchChildSnapshot("steps");

        // validate sra layouts
        var doiLayouts = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.DomainOfInfluence!.ContestId)
            .ThenBy(x => x.VotingCardType)
            .Select(x => new { x.DomainOfInfluence!.Name, x.VotingCardType, x.TemplateId, x.AllowCustom })
            .ToListAsync());
        doiLayouts.ShouldMatchChildSnapshot("doi-layouts");

        // validate attachment counts
        var doiAttachmentCounts = await RunOnDb(db => db.DomainOfInfluenceAttachmentCounts
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.Attachment!.Name)
            .Select(x => new { x.DomainOfInfluence!.Name, x.DomainOfInfluence.ContestId, x.DomainOfInfluence.BasisDomainOfInfluenceId, x.RequiredCount, x.RequiredForVoterListsCount })
            .ToListAsync());
        doiAttachmentCounts.ShouldMatchChildSnapshot("doi-attachment-counts");
    }

    [Fact]
    public async Task DomainOfInfluenceCantonUpdated()
    {
        var eventData = new DomainOfInfluenceUpdated
        {
            DomainOfInfluence = new DomainOfInfluenceEventData
            {
                Id = DomainOfInfluenceMockData.BundId,
                Name = "Bund updated",
                AuthorityName = "Bund updated",
                ShortName = "CH",
                SecureConnectId = "1234",
                ResponsibleForVotingCards = true,
                Canton = DomainOfInfluenceCanton.Tg,
            },
        };

        await TestEventPublisher.Publish(eventData);

        var dois = await RunOnDb(db => db.DomainOfInfluences.Where(x => x.RootId == DomainOfInfluenceMockData.BundGuid).ToListAsync());
        dois.All(x => x.Canton == Data.Models.DomainOfInfluenceCanton.Tg).Should().BeTrue();
        dois.All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals(CantonSettingsMockData.Thurgau.VotingDocumentsEVotingEaiMessageType)).Should().BeTrue();

        // should update all in testing phase
        // but should leave in testing phase untouched
        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences.AsSplitQuery().Include(x => x.Contest).ToListAsync());
        var contestDoisByTestingPhaseEnded = contestDois
            .Where(x => x.RootId == StimmunterlagenUuidV5.BuildContestDomainOfInfluence(x.ContestId, DomainOfInfluenceMockData.BundGuid))
            .GroupBy(x => x.Contest!.TestingPhaseEnded)
            .ToDictionary(x => x.Key, x => x.ToList());
        contestDoisByTestingPhaseEnded[true].All(x => !x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals(CantonSettingsMockData.Thurgau.VotingDocumentsEVotingEaiMessageType)).Should().BeTrue();
        contestDoisByTestingPhaseEnded[true].All(x => x.Canton == Data.Models.DomainOfInfluenceCanton.Sg).Should().BeTrue();
        contestDoisByTestingPhaseEnded[false].All(x => x.CantonDefaults.VotingDocumentsEVotingEaiMessageType.Equals(CantonSettingsMockData.Thurgau.VotingDocumentsEVotingEaiMessageType)).Should().BeTrue();
        contestDoisByTestingPhaseEnded[false].All(x => x.Canton == Data.Models.DomainOfInfluenceCanton.Tg).Should().BeTrue();
    }

    [Fact]
    public async Task DomainOfInfluenceCountingCircleEntriesUpdated()
    {
        var id = DomainOfInfluenceMockData.StadtGossauGuid;
        var eventData = new DomainOfInfluenceCountingCircleEntriesUpdated()
        {
            DomainOfInfluenceCountingCircleEntries = new DomainOfInfluenceCountingCircleEntriesEventData
            {
                Id = id.ToString(),
                CountingCircleIds = { CountingCircleMockData.GemeindeArneggId },
            },
        };

        var prevPbPermissionsCount = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync());

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        // validate doi itself
        var doi = await RunOnDb(db => db.DomainOfInfluences.FirstAsync(x => x.Id == id));
        doi.ShouldMatchChildSnapshot("doi");

        // validate assigned counting circles
        var doiCcs = await RunOnDb(db => db.DomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluenceId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        doiCcs.ShouldMatchChildSnapshot("doi_ccs");

        // validate updated in contests
        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == id)
            .OrderBy(x => x.ContestId)
            .Select(x => new { x.ContestId, x.BasisDomainOfInfluenceId, x.Role, x.Name })
            .ToListAsync());
        contestDois.ShouldMatchChildSnapshot("contest_doi");

        // validate assigned contest counting circles
        var contestDoiCcs = await RunOnDb(db => db.ContestDomainOfInfluenceCountingCircles
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == id)
            .Select(x => new { x.DomainOfInfluenceId, x.CountingCircleId })
            .OrderBy(x => x.DomainOfInfluenceId)
            .ToListAsync());
        contestDoiCcs.ShouldMatchChildSnapshot("contest_doi_ccs");

        // validate update pb permissions if cc is assigned to a new domain of influence
        var updatedPbPermissionsCount = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .CountAsync());

        prevPbPermissionsCount.Should().Be(updatedPbPermissionsCount - 2);
    }

    [Fact]
    public async Task DomainOfInfluenceVotingCardDataUpdated()
    {
        var guid = DomainOfInfluenceMockData.StadtGossauGuid;

        var prevPbPermissionsCount = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
            .CountAsync());

        var eventData = new DomainOfInfluenceVotingCardDataUpdated
        {
            DomainOfInfluenceId = guid.ToString(),
            PrintData = new DomainOfInfluenceVotingCardPrintDataEventData
            {
                ShippingAway = VotingCardShippingFranking.A,
                ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen,
                ShippingReturn = VotingCardShippingFranking.GasB,
                ShippingVotingCardsToDeliveryAddress = true,
            },
            SwissPostData = new DomainOfInfluenceVotingCardSwissPostDataEventData
            {
                InvoiceReferenceNumber = "049844412",
                FrankingLicenceAwayNumber = "70000001",
                FrankingLicenceReturnNumber = "094922284",
            },
            ReturnAddress = new DomainOfInfluenceVotingCardReturnAddressEventData
            {
                City = "St. Gallen",
                Country = "Switzerland",
                Street = "Bahnhofplatz",
                ZipCode = "9000",
                AddressLine1 = "Rathaus",
            },
            ExternalPrintingCenter = false,
            ExternalPrintingCenterEaiMessageType = "EAI-Gossau",
            StistatMunicipality = false,
            VotingCardFlatRateDisabled = false,
            IsMainVotingCardsDomainOfInfluence = false,
            HasEmptyVotingCards = false,
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        var doi = await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.StadtGossauGuid));
        doi.PrintData.Should().NotBeNull();
        doi.PrintData!.ShouldMatchChildSnapshot("PrintData");
        doi.ReturnAddress.Should().NotBeNull();
        doi.ReturnAddress!.ShouldMatchChildSnapshot("ReturnAddress");
        doi.SwissPostData.Should().NotBeNull();
        doi.SwissPostData!.ShouldMatchChildSnapshot("SwissPostData");
        doi.ExternalPrintingCenter.Should().BeFalse();
        doi.ExternalPrintingCenterEaiMessageType.Should().Be("EAI-Gossau");
        doi.StistatMunicipality.Should().BeFalse();
        doi.VotingCardFlatRateDisabled.Should().BeFalse();
        doi.IsMainVotingCardsDomainOfInfluence.Should().BeFalse();
        doi.HasEmptyVotingCards.Should().BeFalse();

        var contestDois = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == guid && x.Contest!.State <= ContestState.TestingPhase)
            .Include(x => x.PrintJob)
            .ToListAsync());

        foreach (var contestDoi in contestDois)
        {
            contestDoi.PrintData.Should().NotBeNull();
            contestDoi.PrintData!.ShouldMatchChildSnapshot("PrintData");
            contestDoi.ReturnAddress.Should().NotBeNull();
            contestDoi.ReturnAddress!.ShouldMatchChildSnapshot("ReturnAddress");
            contestDoi.StistatMunicipality.Should().BeFalse();
            contestDoi.VotingCardFlatRateDisabled.Should().BeFalse();
            contestDoi.IsMainVotingCardsDomainOfInfluence.Should().BeFalse();
            contestDoi.HasEmptyVotingCards.Should().BeFalse();
        }

        contestDois.Select(x => x.PrintJob).WhereNotNull().Any().Should().BeTrue();

        // test update pb permissions if main voting cards flag changes
        var updatedPbPermissionsCount = await RunOnDb(db => db.PoliticalBusinessPermissions
            .Where(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid)
            .CountAsync());

        prevPbPermissionsCount.Should().Be(updatedPbPermissionsCount + 1);

        // test attachment delete with external printing center
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid;
        var attachmentGuid = new Guid("d2c95509-9949-48c5-9997-9eb64aade139");
        var parentAttachmentGuid = AttachmentMockData.BundFutureApprovedBund1Guid;

        await RunOnDb(async db =>
        {
            db.Attachments.Add(new()
            {
                Id = attachmentGuid,
                DomainOfInfluenceId = doiGuid,
            });

            var count = await db.DomainOfInfluenceAttachmentCounts
                .AsTracking()
                .SingleAsync(x => x.DomainOfInfluenceId == doiGuid && x.AttachmentId == parentAttachmentGuid);

            count.RequiredCount = 10;
            count.RequiredForVoterListsCount = 15;

            await db.SaveChangesAsync();
        });

        await RunScoped<AttachmentRepo>(repo => repo.UpdateTotalCountsForDomainOfInfluence(doiGuid));
        var parentAttachment = await GetDbEntity<Attachment>(a => a.Id == parentAttachmentGuid);
        parentAttachment.TotalRequiredCount.Should().Be(50);
        parentAttachment.TotalRequiredForVoterListsCount.Should().Be(15);

        var eventDataWithExternalPrintingCenter = new DomainOfInfluenceVotingCardDataUpdated
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.StadtGossauId,
            PrintData = new DomainOfInfluenceVotingCardPrintDataEventData
            {
                ShippingAway = VotingCardShippingFranking.A,
                ShippingMethod = VotingCardShippingMethod.PrintingPackagingShippingToCitizen,
                ShippingReturn = VotingCardShippingFranking.GasB,
            },
            SwissPostData = new DomainOfInfluenceVotingCardSwissPostDataEventData
            {
                InvoiceReferenceNumber = "049844412",
                FrankingLicenceAwayNumber = "79912300",
                FrankingLicenceReturnNumber = "094922284",
            },
            ReturnAddress = new DomainOfInfluenceVotingCardReturnAddressEventData
            {
                City = "St. Gallen",
                Country = "Switzerland",
                Street = "Bahnhofplatz",
                ZipCode = "9000",
                AddressLine1 = "Rathaus",
            },
            ExternalPrintingCenter = true,
            ExternalPrintingCenterEaiMessageType = "EAI-Gossau Update",
            StistatMunicipality = true,
            VotingCardFlatRateDisabled = true,
        };

        await TestEventPublisher.PublishTwice(eventDataWithExternalPrintingCenter);

        var contestDoisWithExternalPrintingCenter = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == guid && x.Contest!.State <= ContestState.TestingPhase)
            .ToListAsync());
        foreach (var contestDoi in contestDoisWithExternalPrintingCenter)
        {
            contestDoi.ExternalPrintingCenter.Should().BeTrue();
            contestDoi.ExternalPrintingCenterEaiMessageType.Should().Be("EAI-Gossau Update");
            contestDoi.StistatMunicipality.Should().BeTrue();
            contestDoi.VotingCardFlatRateDisabled.Should().BeTrue();
        }

        contestDoisWithExternalPrintingCenter.Select(x => x.PrintJob).WhereNotNull().Any().Should().BeFalse();

        // validate attachment deleted even as a pb attendee with external printing center.
        // only the doi attachment count on parent attachments should remain.
        (await RunOnDb(db => db.Attachments.AnyAsync(a => a.Id == attachmentGuid)))
            .Should().BeFalse();

        // attachment counts of the external printing center attendee doi should not be subtracted from the total attachment counts when it is an pb attendee.
        var parentAttachmentAfterUpdateWithExternalPrintingCenter = await RunOnDb(db => db.Attachments!
            .Include(a => a.DomainOfInfluenceAttachmentCounts!)
            .ThenInclude(x => x.DomainOfInfluence)
            .FirstAsync(a => a.Id == parentAttachmentGuid));

        parentAttachmentAfterUpdateWithExternalPrintingCenter.TotalRequiredCount.Should().Be(50);
        parentAttachmentAfterUpdateWithExternalPrintingCenter.TotalRequiredForVoterListsCount.Should().Be(15);

        // validate steps, should still have the attachment step when it is an pb attendee with external printing center.
        var hasAttachmentStep = await RunOnDb(db => db.StepStates
                    .AnyAsync(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == guid && x.Step == Data.Models.Step.Attachments));
        hasAttachmentStep.Should().BeTrue();
    }

    [Fact]
    public async Task DomainOfInfluenceLogoDeleted()
    {
        var eventData = new DomainOfInfluenceLogoDeleted
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.StadtGossauId,
            LogoRef = "my-logo",
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        var doi = await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.StadtGossauGuid));
        doi.LogoRef.Should().BeNull();

        var logoRef = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.StadtGossauGuid && x.Contest!.State <= ContestState.TestingPhase)
            .Select(x => x.LogoRef)
            .Distinct()
            .SingleAsync());
        logoRef.Should().BeNull();
    }

    [Fact]
    public async Task DomainOfInfluenceLogoUpdated()
    {
        var eventData = new DomainOfInfluenceLogoUpdated
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.StadtGossauId,
            LogoRef = "my-logo",
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        var doi = await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.StadtGossauGuid));
        doi.LogoRef.Should().Be("my-logo");

        var logoRef = await RunOnDb(db => db.ContestDomainOfInfluences
            .Where(x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.StadtGossauGuid && x.Contest!.State <= ContestState.TestingPhase)
            .Select(x => x.LogoRef)
            .Distinct()
            .SingleAsync());
        logoRef.Should().Be("my-logo");
    }

    [Fact]
    public async Task DomainOfInfluenceDeleted()
    {
        var eventData = new DomainOfInfluenceDeleted
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.StadtGossauId,
        };

        // publish two events to test idempotency
        await TestEventPublisher.PublishTwice(eventData);

        (await RunOnDb(db => db.DomainOfInfluences.AnyAsync(x => x.Id == DomainOfInfluenceMockData.StadtGossauGuid)))
            .Should()
            .BeFalse();

        var contestsNotInTestingPhaseCount = await RunOnDb(db => db.Contests.WhereInTestingPhase(false).CountAsync());
        var gossauCount = await RunOnDb(db => db.ContestDomainOfInfluences
            .CountAsync(x => x.BasisDomainOfInfluenceId == DomainOfInfluenceMockData.StadtGossauGuid));
        gossauCount.Should().Be(contestsNotInTestingPhaseCount);
    }
}
