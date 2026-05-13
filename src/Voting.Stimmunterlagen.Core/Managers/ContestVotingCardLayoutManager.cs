// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ContestVotingCardLayoutManager
{
    private readonly IAuth _auth;
    private readonly IDbRepository<ContestVotingCardLayout> _contestLayoutRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardLayout> _doiLayoutRepo;
    private readonly DomainOfInfluenceVotingCardLayoutManager _doiLayoutManager;
    private readonly TemplateManager _templateManager;
    private readonly ContestManager _contestManager;
    private readonly DataContext _dbContext;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public ContestVotingCardLayoutManager(
        IAuth auth,
        IDbRepository<ContestVotingCardLayout> contestLayoutRepo,
        IDbRepository<DomainOfInfluenceVotingCardLayout> doiLayoutRepo,
        TemplateManager templateManager,
        DomainOfInfluenceVotingCardLayoutManager doiLayoutManager,
        ContestManager contestManager,
        DataContext dbContext,
        IClock clock,
        IMapper mapper)
    {
        _auth = auth;
        _contestLayoutRepo = contestLayoutRepo;
        _doiLayoutRepo = doiLayoutRepo;
        _templateManager = templateManager;
        _doiLayoutManager = doiLayoutManager;
        _contestManager = contestManager;
        _dbContext = dbContext;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task SetLayout(Guid contestId, VotingCardType vcType, bool allowCustom, int templateId, VotingCardLayoutDataConfiguration dataConfiguration)
    {
        var existingLayout = await _contestLayoutRepo.Query()
            .AsTracking()
            .WhereContestNotLocked()
            .WhereIsContestManager(_auth.Tenant.Id)
            .WhereContestApproved(false)
            .WhereContestPrintingCenterSignUpDeadlineNotSetOrNotPast(_clock)
            .Include(x => x.Contest!.DomainOfInfluence)
            .FirstOrDefaultAsync(x => x.VotingCardType == vcType && x.ContestId == contestId)
            ?? throw new EntityNotFoundException(nameof(ContestVotingCardLayout), new { contestId, vcType });

        Contest contest = await _contestManager.Get(contestId, false);
        if (contest.DomainOfInfluence!.StistatMunicipality && !contest!.IsPoliticalAssembly)
        {
            dataConfiguration.IncludePersonId = true;
            dataConfiguration.IncludeDateOfBirth = true;
        }

        var template = await _templateManager.GetOrCreateTemplate(templateId);
        existingLayout.AllowCustom = allowCustom;
        existingLayout.TemplateId = templateId;
        existingLayout.DataConfiguration = dataConfiguration;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var doiLayouts = await _doiLayoutRepo.Query()
            .AsTracking()
            .Include(x => x.TemplateDataFieldValues)
            .Include(x => x.DomainOfInfluence)
            .Where(x => x.VotingCardType == vcType && x.DomainOfInfluence!.ContestId == contestId)
            .WhereGenerateVotingCardsTriggered(false)
            .ToListAsync();

        foreach (var doiLayout in doiLayouts)
        {
            if (doiLayout.EffectiveTemplateId != templateId)
            {
                _doiLayoutManager.SyncTemplateFields(doiLayout, template);
            }

            doiLayout.DomainOfInfluenceTemplateId = null;
            doiLayout.OverriddenTemplateId = null;
            doiLayout.TemplateId = existingLayout.TemplateId;
            doiLayout.AllowCustom = existingLayout.AllowCustom;
            doiLayout.DataConfiguration = _mapper.Map<VotingCardLayoutDataConfiguration>(dataConfiguration);
            if (doiLayout.DomainOfInfluence!.StistatMunicipality && !contest.IsPoliticalAssembly)
            {
                doiLayout.DataConfiguration.IncludePersonId = true;
                doiLayout.DataConfiguration.IncludeDateOfBirth = true;
            }
        }

        await _doiLayoutRepo.SaveChanges();
        await transaction.CommitAsync();
    }

    public Task<List<ContestVotingCardLayout>> GetLayouts(Guid contestId)
    {
        return _contestLayoutRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .Where(x => x.ContestId == contestId)
            .Include(x => x.Template)
            .OrderBy(x => x.VotingCardType)
            .ToListAsync();
    }

    public async Task<List<Template>> GetTemplates(Guid contestId)
    {
        await _contestManager.EnsureIsContestManager(contestId);
        return await _templateManager.GetTemplates();
    }

    public async Task<Stream> GetPdfPreview(Guid contestId, VotingCardType vcType, CancellationToken ct)
    {
        var layout = await _contestLayoutRepo.Query()
                             .Include(x => x.Contest)
                             .WhereIsContestManager(_auth.Tenant.Id)
                             .Where(x => x.ContestId == contestId)
                             .Where(x => x.VotingCardType == vcType)
                             .FirstOrDefaultAsync(ct)
                         ?? throw new EntityNotFoundException(nameof(ContestVotingCardLayout), new { contestId, vcType });

        if (layout.TemplateId == null)
        {
            throw new EntityNotFoundException(nameof(layout.Template), new { contestId, vcType });
        }

        return await _templateManager.GetPdfPreview(null, layout.TemplateId.Value, layout.Contest!, layout.DataConfiguration, cancellationToken: ct);
    }
}
