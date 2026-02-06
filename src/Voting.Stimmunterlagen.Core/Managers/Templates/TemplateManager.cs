// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Lib.Database.Repositories;
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Exceptions;
using Voting.Lib.DmDoc.Models;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Models.TemplateData;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using Template = Voting.Stimmunterlagen.Data.Models.Template;
using Voter = Voting.Stimmunterlagen.Data.Models.Voter;
using VotingCardLayoutDataConfiguration = Voting.Stimmunterlagen.Data.Models.VotingCardLayoutDataConfiguration;

namespace Voting.Stimmunterlagen.Core.Managers.Templates;

public class TemplateManager
{
    private const string SubTemplatePrefix = "sub_";
    private const string TenantCategoryPrefix = "tenantId_";
    private const string BrickNameSeparator = "__";
    private const string SerialLetterBulkRoot = "$.data." + TemplateBag.VoterContainerName;

    private readonly IDmDocService _dmDoc;
    private readonly IDbRepository<DataContext, int, Template> _templateRepo;
    private readonly TemplateDataContainerRepo _templateDataContainerRepo;
    private readonly TemplateDataBuilder _templateDataBuilder;
    private readonly IAuth _auth;
    private readonly ILogger<TemplateManager> _logger;

    public TemplateManager(
        IDmDocService dmDoc,
        IDbRepository<DataContext, int, Template> templateRepo,
        TemplateDataContainerRepo templateDataContainerRepo,
        TemplateDataBuilder templateDataBuilder,
        ILogger<TemplateManager> logger,
        IAuth auth)
    {
        _dmDoc = dmDoc;
        _templateRepo = templateRepo;
        _templateDataBuilder = templateDataBuilder;
        _logger = logger;
        _auth = auth;
        _templateDataContainerRepo = templateDataContainerRepo;
    }

    internal async Task<List<Template>> GetTemplates()
    {
        var categories = await _dmDoc.ListCategories();
        var accessibleCategoryInternNames = GetValidCategories(categories)
                                    .Select(category => category.InternName)
                                    .Distinct();
        var allTemplates = await Task.WhenAll(accessibleCategoryInternNames.Select(GetCategoriesTemplatesIgnore404));
        return allTemplates.SelectMany(x => x)
            .Where(FilterTemplates)
            .OrderBy(t => t.Name)
            .Select(t => new Template { Id = t.Id, Name = t.Name, InternName = t.InternName })
            .ToList();
    }

    internal async Task<List<Template>> GetOrCreateTemplates(IEnumerable<int> ids)
    {
        var templates = await _templateRepo.Query()
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.DataContainers!).ThenInclude(c => c.Fields)
            .ToListAsync();

        await Task.WhenAll(templates.Select(EnsureHasAccessToTemplate));

        var missingTemplateIds = ids.Except(templates.Select(t => t.Id)).ToList();
        foreach (var missingTemplateId in missingTemplateIds)
        {
            templates.Add(await CreateTemplate(missingTemplateId));
        }

        return templates;
    }

    internal async Task<Template> GetOrCreateTemplate(int id)
    {
        var template = await _templateRepo.Query()
            .Include(x => x.DataContainers!).ThenInclude(c => c.Fields)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (template == null)
        {
            return await CreateTemplate(id);
        }

        await EnsureHasAccessToTemplate(template);
        return template;
    }

    internal Task<Stream> GetPdfPreview(
        DateTime? contestDate,
        DomainOfInfluenceVotingCardLayout layout,
        IEnumerable<Voter>? voters = null,
        CancellationToken ct = default)
        => GetPdfPreview(
            contestDate,
            layout.EffectiveTemplateId ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardLayout), new { layout.DomainOfInfluenceId, layout.VotingCardType }),
            layout.DomainOfInfluence!.Contest!,
            layout.DataConfiguration,
            layout.DomainOfInfluence!,
            layout.TemplateDataFieldValues!,
            voters,
            ct);

    internal async Task<Stream> GetPdfPreview(
        DateTime? contestDate,
        int templateId,
        Contest contest,
        VotingCardLayoutDataConfiguration dataConfig,
        ContestDomainOfInfluence? domainOfInfluence = null,
        IEnumerable<TemplateDataFieldValue>? data = null,
        IEnumerable<Voter>? voters = null,
        CancellationToken cancellationToken = default)
    {
        domainOfInfluence ??= _templateDataBuilder.GetDummyDomainOfInfluence(_auth.Tenant.Id);
        var templateBag = await BuildTemplateBag(contestDate, templateId, contest, dataConfig, domainOfInfluence, data, voters, cancellationToken);
        return await _dmDoc.PreviewAsPdf(templateId, templateBag, SerialLetterBulkRoot, cancellationToken);
    }

    internal async Task<Stream> GetPdf(
        DateTime? contestDate,
        DomainOfInfluenceVotingCardLayout layout,
        IEnumerable<Voter>? voters,
        CancellationToken ct)
    {
        var templateId = layout.EffectiveTemplateId
            ?? throw new EntityNotFoundException(
                nameof(DomainOfInfluenceVotingCardLayout),
                new { layout.DomainOfInfluenceId, layout.VotingCardType });

        var templateBag = await BuildTemplateBag(
            contestDate,
            templateId,
            layout.DomainOfInfluence!.Contest!,
            layout.DataConfiguration,
            layout.DomainOfInfluence,
            layout.TemplateDataFieldValues!,
            voters,
            ct);
        return await _dmDoc.FinishAsPdf(templateId, templateBag, SerialLetterBulkRoot, ct);
    }

    internal async Task<int> StartPdfGeneration(
        DateTime? contestDate,
        DomainOfInfluenceVotingCardLayout layout,
        IEnumerable<Voter>? voters,
        string webhookUrl,
        CancellationToken ct)
    {
        var templateId = layout.EffectiveTemplateId
            ?? throw new EntityNotFoundException(
                nameof(DomainOfInfluenceVotingCardLayout),
                new { layout.DomainOfInfluenceId, layout.VotingCardType });

        var templateBag = await BuildTemplateBag(
            contestDate,
            templateId,
            layout.DomainOfInfluence!.Contest!,
            layout.DataConfiguration,
            layout.DomainOfInfluence,
            layout.TemplateDataFieldValues!,
            voters,
            ct);
        var draft = await _dmDoc.StartAsyncPdfGeneration(templateId, templateBag, webhookUrl, SerialLetterBulkRoot, ct);
        return draft.Id;
    }

    internal Task<Stream> GetPdfForPrintJob(int printJobId, CancellationToken ct)
        => _dmDoc.GetPdfForPrintJob(printJobId, ct);

    internal async Task<List<TemplateBrick>> GetBricksForMyTenant(int? templateId, bool forActiveBricks = false)
    {
        if (templateId == null)
        {
            throw new ArgumentNullException(nameof(templateId));
        }

        var template = await GetOrCreateTemplate(templateId.Value);
        if (string.IsNullOrWhiteSpace(template.InternName))
        {
            throw new InvalidOperationException($"Template with id {templateId} has no intern name");
        }

        IEnumerable<Brick>[] allBricks = await GetAllTenantBricks(forActiveBricks);
        var templateSuffix = $"{BrickNameSeparator}{TenantCategoryPrefix}{_auth.Tenant.Id}";
        return allBricks.SelectMany(x => x)
            .Where(x => x.InternName.StartsWith($"template_{template.InternName}{BrickNameSeparator}") && x.InternName.EndsWith(templateSuffix))
            .OrderBy(t => t.Name)
            .Select(MapToTemplateBrick)
            .ToList();
    }

    internal async Task<string> GetBrickContentEditorUrl(int brickId, int brickContentId)
    {
        await EnsureHasAccessToBrickContent(brickContentId);
        return await _dmDoc.GetBrickContentEditorUrl(brickId, brickContentId);
    }

    internal async Task<(int NewBrickId, int NewContentId)> UpdateBrickContent(int brickContentId, string content)
    {
        await EnsureHasAccessToBrickContent(brickContentId);
        return await _dmDoc.UpdateBrickContent(brickContentId, content);
    }

    internal async Task TagBricks(DomainOfInfluenceVotingCardLayout layout)
    {
        var bricks = await GetBricksForMyTenant(layout!.EffectiveTemplateId, true);
        if (bricks.Count == 0)
        {
            throw new DmDocException($"No bricks available for tennant {_auth.Tenant.Id}");
        }

        await _dmDoc.TagBricks(bricks.Select(b => b.Id).ToArray(), layout.DomainOfInfluence!.Contest!.Date.ToString("dd.MM.yyyy"));
    }

    private async Task<IEnumerable<Brick>[]> GetAllTenantBricks(bool forActiveBricks = false)
    {
        var categories = await _dmDoc.ListCategories();
        var accessibleCategoryInternNames = GetValidCategories(categories)
                                    .Select(category => category.InternName)
                                    .Distinct();
        if (forActiveBricks)
        {
            return await Task.WhenAll(accessibleCategoryInternNames.Select(GetCategoriesActiveBricksIgnore404));
        }

        return await Task.WhenAll(accessibleCategoryInternNames.Select(GetCategoriesBricksIgnore404));
    }

    private async Task EnsureHasAccessToBrickContent(int brickContentId)
    {
        IEnumerable<Brick>[] allBricks = await GetAllTenantBricks();
        var accessibleBrickContentIds = allBricks
            .SelectMany(x => x)
            .Select(MapToTemplateBrick)
            .Where(x => x.ContentId == brickContentId)
            .Select(x => x.ContentId)
            .ToList();

        if (!accessibleBrickContentIds.Contains(brickContentId))
        {
            throw new DmDocException($"Brick with content id {brickContentId} not found");
        }
    }

    private IEnumerable<Category> GetValidCategories(IEnumerable<Category> categories)
    {
        foreach (var category in categories)
        {
            if (category.Access == true)
            {
                yield return category;
            }

            if (category.Children?.Count > 0)
            {
                foreach (var child in GetValidCategories(category.Children))
                {
                    yield return child;
                }
            }
        }
    }

    private async Task<TemplateBagWrapper> BuildTemplateBag(
        DateTime? contestDate,
        int templateId,
        Contest contest,
        VotingCardLayoutDataConfiguration dataConfig,
        ContestDomainOfInfluence? domainOfInfluence,
        IEnumerable<TemplateDataFieldValue>? data,
        IEnumerable<Voter>? voters,
        CancellationToken cancellationToken = default)
    {
        if (data != null)
        {
            return await _templateDataBuilder.BuildBag(contestDate, contest, dataConfig, domainOfInfluence, voters, data);
        }

        var template = await _templateRepo.Query()
                .Include(x => x.DataContainers!).ThenInclude(c => c.Fields)
                .FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Template), templateId);
        return await _templateDataBuilder.BuildBag(contestDate, contest, dataConfig, domainOfInfluence, voters, template.DataContainers!);
    }

    private async Task<Template> CreateTemplate(int id)
    {
        var dmDocTemplate = await _dmDoc.GetTemplate(id);
        var dataContainers = await _dmDoc.ListTemplateDataContainers(id);

        var templateDataContainers = dataContainers
            .Where(d => !d.Global)
            .Select(MapToTemplateDataContainer)
            .ToList();

        templateDataContainers = await _templateDataContainerRepo.Merge(templateDataContainers);

        var template = new Template
        {
            Id = id,
            Name = dmDocTemplate.Name,
            InternName = dmDocTemplate.InternName,
            DataContainers = templateDataContainers,
        };

        await _templateRepo.Create(template);
        return template;
    }

    private async Task EnsureHasAccessToTemplate(Template template)
    {
        try
        {
            await _dmDoc.GetTemplate(template.Id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Could not check access on template");
            throw new ForbiddenException("no access to the template with id " + template.Id);
        }
    }

    private async Task<IEnumerable<Lib.DmDoc.Models.Template>> GetCategoriesTemplatesIgnore404(string categoryInternName)
    {
        try
        {
            return await _dmDoc.ListTemplates(categoryInternName);
        }
        catch (DmDocException e) when (e.InnerException is HttpRequestException { StatusCode: HttpStatusCode.NotFound })
        {
            _logger.LogWarning(e, "Could not resolve templates for tenant {TenantId} and category {Category}", _auth.Tenant.Id, categoryInternName);
            return Enumerable.Empty<Lib.DmDoc.Models.Template>();
        }
    }

    private async Task<IEnumerable<Lib.DmDoc.Models.Brick>> GetCategoriesBricksIgnore404(string categoryInternName)
    {
        try
        {
            return await _dmDoc.ListBricks(categoryInternName);
        }
        catch (DmDocException e) when (e.InnerException is HttpRequestException { StatusCode: HttpStatusCode.NotFound })
        {
            _logger.LogWarning(e, "Could not resolve brick for tenant {TenantId} and category {Category}", _auth.Tenant.Id, categoryInternName);
            return Enumerable.Empty<Lib.DmDoc.Models.Brick>();
        }
    }

    private async Task<IEnumerable<Lib.DmDoc.Models.Brick>> GetCategoriesActiveBricksIgnore404(string categoryInternName)
    {
        try
        {
            return await _dmDoc.ListActiveBricks(categoryInternName);
        }
        catch (DmDocException e) when (e.InnerException is HttpRequestException { StatusCode: HttpStatusCode.NotFound })
        {
            _logger.LogWarning(e, "Could not resolve brick for tenant {TenantId} and category {Category}", _auth.Tenant.Id, categoryInternName);
            return Enumerable.Empty<Lib.DmDoc.Models.Brick>();
        }
    }

    /// <summary>
    /// Predicate to filter templates not relevant for stimmunterlagen.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>True if the template should be included.</returns>
    private bool FilterTemplates(Lib.DmDoc.Models.Template template)
        => !template.Name.StartsWith(SubTemplatePrefix);

    private TemplateDataContainer MapToTemplateDataContainer(DataContainer dataContainer)
    {
        return new TemplateDataContainer
        {
            Id = dataContainer.Id,
            Key = dataContainer.InternName,
            Name = dataContainer.DataContainerName,
            Fields = dataContainer.Fields.ConvertAll(f => new TemplateDataField
            {
                Key = f.Key,
                Name = f.Name,
            }),
        };
    }

    private TemplateBrick MapToTemplateBrick(Brick brick)
    {
        return new TemplateBrick(brick.Id, brick.Name, brick.Description, brick.PreviewData, brick.BrickContents.First().Id);
    }
}
