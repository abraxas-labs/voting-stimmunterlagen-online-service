// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class DomainOfInfluenceCantonDefaultsBuilder
{
    private readonly DomainOfInfluenceRepo _doiRepo;
    private readonly ContestDomainOfInfluenceRepo _contestDoiRepo;
    private readonly IDbRepository<DataContext, Contest> _contestRepo;
    private readonly IDbRepository<DataContext, CantonSettings> _cantonSettingsRepo;

    public DomainOfInfluenceCantonDefaultsBuilder(DomainOfInfluenceRepo doiRepo, IDbRepository<DataContext, CantonSettings> cantonSettingsRepo, ContestDomainOfInfluenceRepo contestDoiRepo, IDbRepository<DataContext, Contest> contestRepo)
    {
        _doiRepo = doiRepo;
        _cantonSettingsRepo = cantonSettingsRepo;
        _contestDoiRepo = contestDoiRepo;
        _contestRepo = contestRepo;
    }

    public async Task Update(BaseDomainOfInfluence doi)
    {
        if (doi.Canton == DomainOfInfluenceCanton.Unspecified)
        {
            doi.Canton = await _doiRepo.GetRootCanton(doi.Id);
        }

        var cantonSettings = await LoadCantonSettings(doi.Canton);
        ApplyCantonSettings(doi, cantonSettings);
    }

    public async Task UpdateHierarchicalForRoot(DomainOfInfluence rootDoi)
    {
        var cantonSettings = await LoadCantonSettings(rootDoi.Canton);
        await UpdateHierarchicalDomainOfInfluences(q => q.Where(x => x.RootId == rootDoi.Id), cantonSettings);
        await UpdateHierarchicalContestDomainOfInfluencesForRoot(rootDoi.Id, cantonSettings);
    }

    public async Task UpdateHierarchical(CantonSettings cantonSettings)
    {
        await UpdateHierarchicalDomainOfInfluences(q => q.Where(x => x.Canton == cantonSettings.Canton), cantonSettings);
        await UpdateHierarchicalContestDomainOfInfluencesForCanton(cantonSettings);
    }

    private async Task UpdateHierarchicalDomainOfInfluences(Func<IQueryable<DomainOfInfluence>, IQueryable<DomainOfInfluence>> filter, CantonSettings cantonSettings)
    {
        var dois = await filter(_doiRepo.Query()).ToListAsync();

        foreach (var doi in dois)
        {
            ApplyCantonSettings(doi, cantonSettings);
        }

        await _doiRepo.UpdateRange(dois);
    }

    private async Task UpdateHierarchicalContestDomainOfInfluencesForCanton(CantonSettings cantonSettings)
    {
        await UpdateHierarchicalContestDomainOfInfluences(
            q => q.WhereContestInTestingPhase().Where(x => x.Canton == cantonSettings.Canton),
            cantonSettings);
    }

    private async Task UpdateHierarchicalContestDomainOfInfluencesForRoot(Guid basisRootId, CantonSettings cantonSettings)
    {
        var contestIds = await _contestRepo.Query()
            .WhereInTestingPhase()
            .Select(x => x.Id)
            .ToListAsync();
        var rootIds = contestIds.Select(ccId => StimmunterlagenUuidV5.BuildContestDomainOfInfluence(ccId, basisRootId)).ToHashSet();
        await UpdateHierarchicalContestDomainOfInfluences(q => q.Where(x => rootIds.Contains(x.RootId)), cantonSettings);
    }

    private async Task UpdateHierarchicalContestDomainOfInfluences(Func<IQueryable<ContestDomainOfInfluence>, IQueryable<ContestDomainOfInfluence>> filter, CantonSettings cantonSettings)
    {
        var contestDois = await filter(_contestDoiRepo.Query()).ToListAsync();

        foreach (var contestDoi in contestDois)
        {
            ApplyCantonSettings(contestDoi, cantonSettings);
        }

        await _contestDoiRepo.UpdateRange(contestDois);
    }

    private void ApplyCantonSettings(BaseDomainOfInfluence doi, CantonSettings settings)
    {
        doi.Canton = settings.Canton;
        doi.CantonDefaults = new DomainOfInfluenceCantonDefaults()
        {
            VotingDocumentsEVotingEaiMessageType = settings.VotingDocumentsEVotingEaiMessageType,
        };
    }

    private async Task<CantonSettings> LoadCantonSettings(DomainOfInfluenceCanton canton)
        => await _cantonSettingsRepo.Query().SingleOrDefaultAsync(x => x.Canton == canton)
            ?? new CantonSettings { Canton = canton };
}
