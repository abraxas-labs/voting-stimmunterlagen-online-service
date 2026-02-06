// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class DomainOfInfluenceVotingCardManager
{
    private readonly IAuth _auth;
    private readonly IDbRepository<DomainOfInfluenceVotingCardLayout> _doiLayoutRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardConfiguration> _doiConfigurationRepo;
    private readonly VoterRepo _voterRepo;
    private readonly TemplateManager _templateManager;
    private readonly IClock _clock;

    public DomainOfInfluenceVotingCardManager(
        IAuth auth,
        IDbRepository<DomainOfInfluenceVotingCardLayout> doiLayoutRepo,
        IDbRepository<DomainOfInfluenceVotingCardConfiguration> doiConfigurationRepo,
        VoterRepo voterRepo,
        TemplateManager templateManager,
        IClock clock)
    {
        _auth = auth;
        _doiLayoutRepo = doiLayoutRepo;
        _doiConfigurationRepo = doiConfigurationRepo;
        _voterRepo = voterRepo;
        _templateManager = templateManager;
        _clock = clock;
    }

    public async Task SetConfiguration(Guid doiId, int sampleCount, IEnumerable<VotingCardGroup> groups, IEnumerable<VotingCardSort> sorts)
    {
        var existingConfiguration = await _doiConfigurationRepo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == doiId)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardConfiguration), doiId);

        existingConfiguration.SampleCount = sampleCount;
        existingConfiguration.Groups = groups.ToArray();
        existingConfiguration.Sorts = sorts.ToArray();
        await _doiConfigurationRepo.Update(existingConfiguration);
    }

    public async Task<DomainOfInfluenceVotingCardConfiguration> GetConfiguration(Guid doiId)
    {
        return await _doiConfigurationRepo.Query()
            .Include(x => x.DomainOfInfluence)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == doiId)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardConfiguration), doiId);
    }

    public async Task<Stream> GetPdfPreview(Guid doiId, VotingCardType vcType, CancellationToken ct)
    {
        var config = await _doiConfigurationRepo.Query()
                         .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
                         .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == doiId, ct)
                     ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardConfiguration), doiId);
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

        var voters = await _voterRepo.Query()
            .WhereVotingCardType(vcType)
            .WhereBelongToDomainOfInfluence(doiId)
            .Where(x => x.ListId.HasValue)
            .Include(x => x.DomainOfInfluences)
            .OrderBy(_ => EF.Functions.Random())
            .Take(config.SampleCount)
            .Include(x => x.List)
            .ToListAsync(ct);

        return await _templateManager.GetPdfPreview(
            null,
            layout,
            voters,
            ct);
    }
}
