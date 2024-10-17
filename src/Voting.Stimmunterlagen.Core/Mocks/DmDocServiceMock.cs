// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Exceptions;
using Voting.Lib.DmDoc.Models;
using Voting.Lib.DmDoc.Serialization.Json;

namespace Voting.Stimmunterlagen.Core.Mocks;

/// <summary>
/// Simple mock for the dm dmdoc service.
/// Can be used for tests or if the abraxas vpn or dmdoc is not available.
/// Removed from the compilation by ms build for release configurations.
/// </summary>
public class DmDocServiceMock : IDmDocService
{
    internal const string MockDataSeederTenantId = "mock-data-seeder-tenantId";

    internal static readonly DataContainer[] MockedDataContainers =
    {
        new()
        {
            Id = 1,
            Global = true,
            InternName = "user",
            DataContainerName = "Benutzer",
            Fields =
            {
                new() { Key = "first_name", Name = "Vorname" },
                new() { Key = "last_name", Name = "Nachname" },
            },
        },
        new()
        {
            Id = 2,
            InternName = "urnengang",
            DataContainerName = "Urnengang",
            Fields =
            {
                new() { Key = "urnengang_datum", Name = "Datum" },
            },
        },
        new()
        {
            Id = 3,
            InternName = "adresse_empfaenger",
            DataContainerName = "Adresse Empfänger",
            Fields =
            {
                new() { Key = "first_name", Name = "Vorname" },
                new() { Key = "last_name", Name = "Nachname" },
            },
        },
        new()
        {
            Id = 4,
            InternName = "urne",
            DataContainerName = "Urne",
            Fields =
            {
                new() { Key = "standort", Name = "Standort der Urne" },
                new() { Key = "zeit", Name = "Zeit" },
            },
        },
        new()
        {
            Id = 5,
            InternName = "e_voting",
            DataContainerName = "E-Voting",
            Fields =
            {
                new() { Key = "e_voting", Name = "eVoting Systemname" },
                new() { Key = "domain", Name = "eVoting Domain" },
            },
        },
    };

    // templates with ids <500 are seeded for tests
    internal static readonly MockedTemplate[] Templates =
    {
            new() { Id = 1, Name = "template-001-swiss", InternName = "template-001-swiss" },
            new() { Id = 2, Name = "template-002-evoting", InternName = "template-002-evoting" },
            new() { Id = 3, Name = "template-003-others", InternName = "template-003-others" },
            new() { Id = 4, Name = "template-004-others2", InternName = "template-004-others2" },

            // templates of the doi's
            new() { Id = 100, Name = "template-100-swiss-arnegg", InternName = "template-100-swiss-arnegg" },
            new() { Id = 101, Name = "template-101-swiss-arnegg", InternName = "template-101-swiss-arnegg" },
            new() { Id = 800, Name = "template-800-swiss-arnegg", InternName = "template-800-swiss-arnegg" },
    };

    internal static readonly MockedBrick[] MockedBricks =
    {
        new(
            1,
            "stimmrechtausweis_text_brieflich",
            "Stimmrechtsausweis Text brieflich",
            "<html><body><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"null\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">Unterschreiben Sie die Erklärung zur brieflichen Stimmabgabe.</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"null\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">Legen Sie den/die ausgefüllten Stimmzettel in das beiliegende Stimmzettelkuvert.</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"null\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">Legen Sie das Stimmzettelkuvert mit dem/den Stimmzettel/n sowie den unterschriebenen Stimmrechtsausweis in das Antwortkuvert, mit dem Sie das Stimmmaterial erhalten haben.</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"null\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">Übergeben Sie das Antwortkuvert rechtzeitig der Post. Briefliche Stimmen müssen spätestens am Abstimmungssonntag bis zur Schliessung der Urnen bei der Rücksendeadresse eintreffen.</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"null\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">Das Fensterkuvert kann</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"1\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">unfrankiert der Post übergeben werden</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"1\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">in den Briefkasten der Gemeindeverwaltung eingeworfen werden</p><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"1\" data-default=\"0\" data-bold=\"null\" data-italic=\"null\" data-text-align=\"null\" data-color=\"null\" data-background-color=\"null\" data-indention-width=\"null\" data-indention-level=\"1\" data-hyphenation=\"null\" data-pre-named-string=\"null\" data-post-named-string=\"null\">an der Urne abgegeben werden</p></body></html>",
            501,
            "template-001-swiss",
            "SC-ABX"),

        new(
            2,
            "stimmrechtausweis_text_vorzeitig",
            "Stimmrechtsausweis Text vorzeitig",
            "<html><body><p style=\"font-family:'arialpro',sans-serif;font-size:9px;line-feed:11px;margin-top:0px;margin-bottom:0px;\" data-pf-name=\"voting_9pt\" data-overwrite=\"0\">Am <strong>Donnerstag</strong> und <strong>Freitag</strong> vor dem Abstimmungssonntag kann beim Front-Office, Parterre, während der Bürozeit, vorzeitig gestimmt werden.</p></body></html>",
            502,
            "template-001-swiss",
            "SC-ABX"),

        new(
            101,
            "stimmrechtausweis_zeit_urne",
            "Stimmrechtsausweis Zeit Urne",
            "<html><body>09:00</body></html>",
            601,
            "template-100-swiss-arnegg",
            "SC-ABX"),
    };

    internal static readonly IReadOnlyDictionary<int, MockedTemplate> TemplatesById = Templates.ToDictionary(x => x.Id);
    internal static readonly IReadOnlyDictionary<int, int> BrickContentIdByBrickId = MockedBricks.ToDictionary(x => x.Id, x => x.BrickContents.Single().Id);

    private static readonly string ExamplePdfPath = Path.Combine(Path.GetDirectoryName(typeof(DmDocServiceMock).Assembly.Location)!, "Mocks/example.pdf");
    private static readonly ConcurrentDictionary<int, Draft> Drafts = new();
    private static int _draftIdCounter;
    private readonly ILogger<DmDocServiceMock> _logger;

    public DmDocServiceMock(ILogger<DmDocServiceMock> logger)
    {
        _logger = logger;
    }

    internal static MockedTemplate TemplateSwiss => TemplatesById[1];

    internal static MockedTemplate TemplateEVoting => TemplatesById[2];

    internal static MockedTemplate TemplateOthers => TemplatesById[3];

    internal static MockedTemplate TemplateOthers2 => TemplatesById[4];

    internal static MockedTemplate TemplateSwissArnegg => TemplatesById[100];

    internal static MockedTemplate TemplateSwissArnegg2 => TemplatesById[100];

    internal static MockedTemplate TemplateSwissArneggNotSeeded => TemplatesById[800];

    public Task<List<Category>> ListCategories(CancellationToken ct = default)
        => Task.FromResult(new List<Category> { new() { Text = "Stimmunterlagen", InternName = "Stimmunterlagen", Access = false, Children = new List<Category>() { new() { Text = "Stimmunterlagen", InternName = "Stimmunterlagen", Access = true, Children = new List<Category>() } } } });

    public Task<Template> GetTemplate(int id, CancellationToken ct = default)
    {
        var template = TemplatesById.GetValueOrDefault(id)
                       ?? throw new DmDocException("not found");
        return Task.FromResult<Template>(template);
    }

    public Task<List<Template>> ListTemplates(CancellationToken ct = default)
    {
        var templates = Templates.OfType<Template>().ToList();
        return Task.FromResult(templates);
    }

    public Task<List<Template>> ListTemplates(string category, CancellationToken ct = default)
        => ListTemplates(ct);

    public Task<List<Category>> ListTemplateCategories(int templateId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<List<DataContainer>> ListTemplateDataContainers(
        int templateId,
        bool includeSystemContainer = false,
        bool includeUserContainer = true,
        CancellationToken ct = default)
    {
        var template = TemplatesById.GetValueOrDefault(templateId)
                       ?? throw new DmDocException("not found");
        return Task.FromResult(template.DataContainers);
    }

    public Task<Draft> CreateDraft<T>(
        int templateId,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
    {
        var id = Interlocked.Increment(ref _draftIdCounter);
        var draft = Drafts[id] = new Draft
        {
            Id = id,
            State = DraftState.Editing,
        };

        _logger.LogInformation("DmDoc Draft created with data {Data}", DmDocJsonSerializer.Serialize(templateData));

        return Task.FromResult(draft);
    }

    public Task<Draft> CreateDraft<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
        => CreateDraft(-1, templateData, bulkRoot, ct);

    public Task<Draft> GetDraft(int draftId, CancellationToken ct = default)
    {
        if (!Drafts.TryGetValue(draftId, out var draft))
        {
            throw new DmDocException("not found");
        }

        return Task.FromResult(draft);
    }

    public Task DeleteDraft(int draftId, CancellationToken ct = default)
    {
        if (!Drafts.Remove(draftId, out _))
        {
            throw new DmDocException("not found");
        }

        return Task.CompletedTask;
    }

    public Task DeleteDraftContent(int draftId, CancellationToken ct = default)
    {
        if (!Drafts.ContainsKey(draftId))
        {
            throw new DmDocException("not found");
        }

        return Task.CompletedTask;
    }

    public Task DeleteDraftHard(int draftId, CancellationToken ct = default)
    {
        if (!Drafts.Remove(draftId, out _))
        {
            throw new DmDocException("not found");
        }

        return Task.CompletedTask;
    }

    public Task<Stream> PreviewDraftAsPdf(int draftId, CancellationToken ct = default)
        => GetExamplePdfStream();

    public async Task<Stream> PreviewAsPdf<T>(
        int templateId,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
    {
        var draft = await CreateDraft(templateId, templateData, bulkRoot, ct);
        return await PreviewDraftAsPdf(draft.Id, ct);
    }

    public async Task<Stream> PreviewAsPdf<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
    {
        var draft = await CreateDraft(templateName, templateData, bulkRoot, ct);
        return await PreviewDraftAsPdf(draft.Id, ct);
    }

    public Task<Stream> FinishDraftAsPdf(int draftId, CancellationToken ct = default)
        => GetExamplePdfStream();

    public async Task<Stream> FinishAsPdf<T>(
        int templateId,
        T templateData,
        string? bulkRoot,
        CancellationToken ct = default)
    {
        var draft = await CreateDraft(templateId, templateData, bulkRoot, ct);
        return await FinishDraftAsPdf(draft.Id, ct);
    }

    public async Task<Stream> FinishAsPdf<T>(
        string templateName,
        T templateData,
        string? bulkRoot = null,
        CancellationToken ct = default)
    {
        var draft = await CreateDraft(templateName, templateData, bulkRoot, ct);
        return await FinishDraftAsPdf(draft.Id, ct);
    }

    public Task<Draft> StartAsyncPdfGeneration<T>(
        int templateId,
        T templateData,
        string webhookEndpoint,
        string? bulkRoot = null,
        CancellationToken ct = default)
        => CreateDraft(templateId, templateData, bulkRoot, ct);

    public Task<Draft> StartAsyncPdfGeneration<T>(
        string templateName,
        T templateData,
        string webhookEndpoint,
        string? bulkRoot = null,
        CancellationToken ct = default)
        => CreateDraft(templateName, templateData, bulkRoot, ct);

    public Task<Stream> GetPdfForPrintJob(int printJobId, CancellationToken ct = default)
        => GetExamplePdfStream();

    public Task<List<Brick>> ListBricks(int categoryId, CancellationToken ct = default)
        => ListBricks();

    public Task<List<Brick>> ListBricks(string category, CancellationToken ct = default)
        => ListBricks();

    public Task<string> GetBrickContentEditorUrl(int brickId, int brickContentId, CancellationToken ct = default)
    {
        if (BrickContentIdByBrickId.GetValueOrDefault(brickId) != brickContentId)
        {
            throw new DmDocException("error");
        }

        return Task.FromResult("http://localhost/mock-editor-url?id=" + brickContentId);
    }

    public Task<(int NewBrickId, int NewContentId)> UpdateBrickContent(int brickContentId, string content, CancellationToken ct = default)
    {
        var brick = Array.Find(MockedBricks, b => b.BrickContents.Single().Id == brickContentId)
            ?? throw new DmDocException("error");

        return Task.FromResult((9999, 10000));
    }

    public Task<List<Brick>> ListBricks(CancellationToken ct = default)
        => Task.FromResult(MockedBricks.OfType<Brick>().ToList());

    private Task<Stream> GetExamplePdfStream()
        => Task.FromResult<Stream>(File.OpenRead(ExamplePdfPath));

    internal class MockedTemplate : Template
    {
        public List<DataContainer> DataContainers { get; set; } = MockedDataContainers.ToList();
    }

    internal class MockedBrick : Brick
    {
        public MockedBrick(int id, string internName, string name, string previewData, int contentId, string templateInternName, string tenantId)
        {
            Id = id;
            InternName = $"template_{templateInternName}__{internName}__tenantId_{tenantId}";
            Name = name;
            PreviewData = previewData;
            BrickContents = new() { new() { Id = contentId } };
        }
    }
}
