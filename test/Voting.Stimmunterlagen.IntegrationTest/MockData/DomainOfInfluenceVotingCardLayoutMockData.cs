// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class DomainOfInfluenceVotingCardLayoutMockData
{
    // template data values by ContestDOIId in the form of {ContainerKey}:{FieldKey}={Value}
    private static readonly IReadOnlyDictionary<Guid, string[]> DoiTemplateDataValues =
        new Dictionary<Guid, string[]>
        {
                {
                    DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
                    new[] { "urne:standort=Gemeindehaus", "e_voting:domain=abraxas.ch" }
                },
                {
                    DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
                    new[] { "urne:standort=Turnhalle", "e_voting:domain=evoting.gossau.ch" }
                },
        };

    private static readonly IReadOnlyDictionary<(Guid, Guid, VotingCardType), int> DoiTemplates =
        new Dictionary<(Guid, Guid, VotingCardType), int>
        {
                {
                    (
                        ContestMockData.BundFutureApprovedGuid,
                        DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid,
                        VotingCardType.Swiss
                    ),
                    DmDocServiceMock.TemplateOthers.Id
                },
                {
                    (
                        ContestMockData.BundFutureGuid,
                        DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
                        VotingCardType.Swiss
                    ),
                    DmDocServiceMock.TemplateOthers.Id
                },
                {
                    (
                        ContestMockData.BundFutureGuid,
                        DomainOfInfluenceMockData.ContestBundFutureStadtGossauGuid,
                        VotingCardType.Swiss
                    ),
                    DmDocServiceMock.TemplateOthers.Id
                },
        };

    private static readonly IReadOnlyDictionary<(Guid, Guid, VotingCardType), int> OverriddenTemplates =
        new Dictionary<(Guid, Guid, VotingCardType), int>
        {
                {
                    (
                        ContestMockData.BundFutureApprovedGuid,
                        DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
                        VotingCardType.Swiss
                    ),
                    DmDocServiceMock.TemplateSwissArnegg.Id
                },
                {
                    (
                        ContestMockData.BundFutureGuid,
                        DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
                        VotingCardType.Swiss
                    ),
                    DmDocServiceMock.TemplateSwissArnegg.Id
                },
        };

    public static async Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        // seed layouts
        await runScoped(async sp =>
        {
            var builder = sp.GetRequiredService<DomainOfInfluenceVotingCardLayoutBuilder>();
            foreach (var contest in ContestMockData.All)
            {
                await builder.SyncForContest(contest.Id);
            }
        });

        // add template field values
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();

            var dois = await db.ContestDomainOfInfluences.Include(doi => doi.VotingCardLayouts).ToListAsync();
            var doisById = dois.ToDictionary(x => x.Id);

            var fields = await db.TemplateDataFields.Include(x => x.Container).ToListAsync();
            var fieldsDict = fields.GroupBy(x => x.Container!.Key + ":" + x.Key).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (doiId, values) in DoiTemplateDataValues)
            {
                var doi = doisById[doiId];
                foreach (var value in values)
                {
                    var keyValue = value.Split("=");
                    var key = keyValue[0];
                    var fieldValue = keyValue[1];
                    var valuesToAdd = fieldsDict[key].Select(f => new TemplateDataFieldValue
                    {
                        FieldId = f.Id,
                        Value = fieldValue,
                    });

                    foreach (var layout in doi.VotingCardLayouts!)
                    {
                        layout.TemplateDataFieldValues ??= new List<TemplateDataFieldValue>();
                        layout.TemplateDataFieldValues.AddRange(valuesToAdd);
                    }
                }
            }

            db.UpdateRange(dois);
            await db.SaveChangesAsync();
        });

        // set templates
        await runScoped(async sp =>
        {
            var db = sp.GetRequiredService<DataContext>();
            var manager = sp.GetRequiredService<DomainOfInfluenceVotingCardLayoutManager>();
            var auth = sp.GetRequiredService<IAuthStore>();

            auth.SetValues(
                "mock-token",
                "mock-data-seeder",
                DmDocServiceMock.MockDataSeederTenantId,
                new[] { Auth.Roles.ElectionAdmin, Auth.Roles.PrintJobManager });

            var layouts = await db.DomainOfInfluenceVotingCardLayouts
                .AsTracking()
                .Include(x => x.DomainOfInfluence!.Contest)
                .Include(x => x.TemplateDataFieldValues!)
                .ThenInclude(x => x.Field!.Container!)
                .ToListAsync();

            var layoutsByContest = layouts
                .GroupBy(x => x.DomainOfInfluence!.ContestId)
                .ToDictionary(x => x.Key, x => x.ToList());
            foreach (var contestVotingCardLayout in ContestVotingCardLayoutMockData.All)
            {
                SetValues(layoutsByContest, contestVotingCardLayout);
            }

            var templatesById = await db.Templates
                .Include(x => x.DataContainers!)
                .ThenInclude(x => x.Fields)
                .ToDictionaryAsync(x => x.Id);
            foreach (var layout in layouts)
            {
                var template = layout.EffectiveTemplateId.HasValue
                    ? templatesById[layout.EffectiveTemplateId.Value]
                    : null;
                manager.SyncTemplateFields(layout, template, true);
            }

            await db.SaveChangesAsync();
        });
    }

    private static void SetValues(
        IReadOnlyDictionary<Guid, List<DomainOfInfluenceVotingCardLayout>> layoutsByContest,
        ContestVotingCardLayout contestVotingCardLayout)
    {
        var vcTypeLayouts = layoutsByContest[contestVotingCardLayout.ContestId]
            .Where(x => x.VotingCardType == contestVotingCardLayout.VotingCardType);
        foreach (var layout in vcTypeLayouts)
        {
            layout.AllowCustom = contestVotingCardLayout.AllowCustom;
            layout.TemplateId = contestVotingCardLayout.TemplateId;

            if (DoiTemplates.TryGetValue(
                (contestVotingCardLayout.ContestId, layout.DomainOfInfluenceId, layout.VotingCardType),
                out var doiTemplateId))
            {
                layout.DomainOfInfluenceTemplateId = doiTemplateId;
            }

            if (OverriddenTemplates.TryGetValue(
                (contestVotingCardLayout.ContestId, layout.DomainOfInfluenceId, layout.VotingCardType),
                out var overriddenTemplateId))
            {
                layout.OverriddenTemplateId = overriddenTemplateId;
            }
        }
    }
}
