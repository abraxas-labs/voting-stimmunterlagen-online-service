// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class DomainOfInfluenceVotingCardLayoutManager
{
    private readonly IAuth _auth;
    private readonly IDbRepository<DomainOfInfluenceVotingCardLayout> _doiLayoutRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<TemplateDataFieldValue> _templateValuesRepo;
    private readonly TemplateManager _templateManager;
    private readonly TemplateDataBuilder _templateDataBuilder;
    private readonly ContestManager _contestManager;
    private readonly DataContext _dbContext;
    private readonly IClock _clock;

    public DomainOfInfluenceVotingCardLayoutManager(
        IAuth auth,
        IDbRepository<DomainOfInfluenceVotingCardLayout> doiLayoutRepo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<TemplateDataFieldValue> templateValuesRepo,
        TemplateManager templateManager,
        TemplateDataBuilder templateDataBuilder,
        IClock clock,
        ContestManager contestManager,
        DataContext dbContext)
    {
        _auth = auth;
        _doiLayoutRepo = doiLayoutRepo;
        _templateValuesRepo = templateValuesRepo;
        _templateManager = templateManager;
        _templateDataBuilder = templateDataBuilder;
        _doiRepo = doiRepo;
        _clock = clock;
        _contestManager = contestManager;
        _dbContext = dbContext;
    }

    public async Task SetLayout(Guid doiId, VotingCardType vcType, bool allowCustom, int? templateId)
    {
        var existingLayout = await _doiLayoutRepo.Query()
            .AsTracking()
            .Include(x => x.DomainOfInfluence!.Contest!.DomainOfInfluence!)
            .Include(x => x.DomainOfInfluence!.VotingCardLayouts!).ThenInclude(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .WhereGenerateVotingCardsTriggered(false)
            .WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast(_clock)
            .WhereIsContestManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestIsApproved(false)
            .FirstOrDefaultAsync(x => x.VotingCardType == vcType && x.DomainOfInfluenceId == doiId)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardLayout), new { vcType, doiId });

        var oldEffectiveTemplateId = existingLayout.EffectiveTemplateId;
        existingLayout.OverriddenTemplateId = null;
        existingLayout.AllowCustom = allowCustom;

        Template? template = null;
        if (templateId == null)
        {
            existingLayout.DomainOfInfluenceTemplateId = null;
        }
        else
        {
            template = await _templateManager.GetOrCreateTemplate(templateId.Value);
            existingLayout.DomainOfInfluenceTemplateId = templateId.Value;
        }

        // In case the template was overridden, the effective template has not changed
        if (oldEffectiveTemplateId == existingLayout.EffectiveTemplateId)
        {
            await _doiLayoutRepo.Update(existingLayout);
            return;
        }

        SyncTemplateFields(existingLayout, template);
        await _doiLayoutRepo.SaveChanges();
    }

    public async Task SetOverriddenLayout(Guid doiId, VotingCardType vcType, int? templateId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var existingLayout = await _doiLayoutRepo.Query()
            .AsTracking()
            .Include(x => x.DomainOfInfluence!.Contest!.DomainOfInfluence!)
            .Include(x => x.DomainOfInfluence!.VotingCardLayouts!).ThenInclude(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .Where(x => x.VotingCardType == vcType && x.DomainOfInfluenceId == doiId)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast(_clock)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardLayout), new { vcType, doiId });

        if (!existingLayout.AllowCustom)
        {
            throw new ValidationException("custom layout is not allowed");
        }

        Template? template = null;
        if (templateId == null)
        {
            existingLayout.OverriddenTemplateId = null;
        }
        else
        {
            template = await _templateManager.GetOrCreateTemplate(templateId.Value);
            existingLayout.OverriddenTemplateId = templateId.Value;
        }

        SyncTemplateFields(existingLayout, template, true);
        await _doiLayoutRepo.SaveChanges();

        await transaction.CommitAsync();
    }

    public async Task<List<GroupedDomainOfInfluenceVotingCardLayouts>> GetLayoutsByContest(Guid contestId)
    {
        var layouts = await _doiLayoutRepo.Query()
            .Where(x => x.DomainOfInfluence!.ContestId == contestId)
            .WhereIsContestManager(_auth.Tenant.Id)
            .Include(x => x.Template)
            .Include(x => x.DomainOfInfluenceTemplate)
            .Include(x => x.OverriddenTemplate)
            .Include(x => x.DomainOfInfluence)
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ThenBy(x => x.VotingCardType)
            .ToListAsync();
        return layouts
            .GroupBy(l => l.DomainOfInfluence!.Id)
            .Select(g => new GroupedDomainOfInfluenceVotingCardLayouts(g.First().DomainOfInfluence!, g.ToList()))
            .ToList();
    }

    public Task<List<DomainOfInfluenceVotingCardLayout>> GetLayoutsByDomainOfInfluence(Guid doiId)
    {
        return _doiLayoutRepo.Query()
            .WhereHasDomainOfInfluence(doiId)
            .WhereHasAccess(_auth.Tenant.Id)
            .Include(x => x.Template)
            .Include(x => x.DomainOfInfluenceTemplate)
            .Include(x => x.OverriddenTemplate)
            .OrderBy(x => x.VotingCardType)
            .ToListAsync();
    }

    public async Task<List<Template>> GetTemplates()
    {
        return await _templateManager.GetTemplates();
    }

    public async Task<Stream> GetPdfPreview(Guid doiId, VotingCardType vcType, CancellationToken ct)
    {
        var layout = await _doiLayoutRepo.Query()
                         .Include(x => x.DomainOfInfluence!.Contest)
                         .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
                         .Where(x => x.DomainOfInfluenceId == doiId && x.VotingCardType == vcType)
                         .WhereHasAccess(_auth.Tenant.Id)
                         .FirstOrDefaultAsync(ct)
                     ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardLayout), new { vcType, doiId });

        if (layout.EffectiveTemplateId == null)
        {
            throw new EntityNotFoundException(nameof(layout.EffectiveTemplateId), new { vcType, doiId });
        }

        return await _templateManager.GetPdfPreview(
            null,
            layout,
            null,
            ct);
    }

    public async Task<IEnumerable<GroupedTemplateValues>> GetTemplateData(Guid doiId)
    {
        var templateValues = await _templateValuesRepo.Query()
            .Where(x => x.Layout!.DomainOfInfluenceId == doiId && x.Layout.DomainOfInfluence!.SecureConnectId == _auth.Tenant.Id)
            .Include(x => x.Field!.Container)
            .OrderBy(x => x.Field!.Container!.Key)
            .ThenBy(x => x.Field!.Key)
            .ToListAsync();

        return templateValues
            .DistinctBy(x => new { FieldKey = x.Field!.Key, ContainerKey = x.Field.Container!.Key })
            .GroupBy(x => x.Field!.ContainerId)
            .Select(x => new GroupedTemplateValues(x.First().Field!.Container!, x));
    }

    public async Task SetTemplateData(Guid doiId, IEnumerable<SimpleTemplateFieldValue> newValues)
    {
        var doiExists = await _doiRepo
            .Query()
            .WhereIsManager(_auth.Tenant.Id)
            .WhereContestNotLocked()
            .WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast(_clock)
            .AnyAsync(x => x.Id == doiId);

        if (!doiExists)
        {
            throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        }

        var layouts = await _doiLayoutRepo.Query()
            .AsTracking()
            .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .Where(x => x.DomainOfInfluenceId == doiId)
            .ToListAsync();

        foreach (var layout in layouts)
        {
            var valuesByKey = layout.TemplateDataFieldValues!.ToDictionary(x => (x.Field!.Container!.Key, x.Field!.Key), x => x);

            foreach (var newValue in newValues)
            {
                if (!valuesByKey.TryGetValue((newValue.ContainerKey, newValue.FieldKey), out var value))
                {
                    throw new ValidationException($"value {newValue.ContainerKey}-{newValue.FieldKey} not found");
                }

                value.Value = newValue.Value;
            }
        }

        await _templateValuesRepo.SaveChanges();
    }

    internal void SyncTemplateFields(DomainOfInfluenceVotingCardLayout layout, Template? template, bool useExisting = false)
    {
        if (layout.TemplateDataFieldValues == null)
        {
            throw new InvalidOperationException("The layout template data field values must be loaded");
        }

        var values = layout.TemplateDataFieldValues;

        if (template == null)
        {
            values.Clear();
            return;
        }

        if (template.DataContainers == null)
        {
            throw new InvalidOperationException("The template data containers must be loaded");
        }

        var containers = template.DataContainers
            .DistinctBy(x => x.Id)
            .ToList();

        var newValues = _templateDataBuilder.BuildUserEnteredValues(
            containers,
            useExisting ? values : null);

        values.Clear();
        values.AddRange(newValues);
    }
}
