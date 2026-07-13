// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class DomainOfInfluenceCountingCircleBuilder
{
    private readonly IDbRepository<DomainOfInfluence> _doiRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _contestDoiRepo;
    private readonly DataContext _dbContext;

    public DomainOfInfluenceCountingCircleBuilder(
        IDbRepository<DomainOfInfluence> doiRepo,
        IDbRepository<ContestDomainOfInfluence> contestDoiRepo,
        DataContext dbContext)
    {
        _doiRepo = doiRepo;
        _contestDoiRepo = contestDoiRepo;
        _dbContext = dbContext;
    }

    internal async Task UpdateDomainOfInfluenceCountingCircles(Guid basisDoiId, List<Guid> basisCcIds)
    {
        var (doi, snapshotDois) = await LoadDomainOfInfluences(basisDoiId);

        await UpdateDomainOfInfluenceCountingCircles<DomainOfInfluence, DomainOfInfluenceCountingCircle, DomainOfInfluenceHierarchyEntry>(doi, basisCcIds);

        foreach (var snapshotDoi in snapshotDois)
        {
            var ccIds = basisCcIds.ConvertAll(basisCcId => StimmunterlagenUuidV5.BuildContestCountingCircle(snapshotDoi.ContestId, basisCcId));
            await UpdateDomainOfInfluenceCountingCircles<ContestDomainOfInfluence, ContestDomainOfInfluenceCountingCircle, ContestDomainOfInfluenceHierarchyEntry>(snapshotDoi, ccIds);
        }

        await _dbContext.SaveChangesAsync();
    }

    internal async Task DeleteAssignedAndInheritedCountingCircles(Guid basisDoiId)
    {
        DomainOfInfluence? doi;
        List<ContestDomainOfInfluence>? snapshotDois;

        try
        {
            (doi, snapshotDois) = await LoadDomainOfInfluences(basisDoiId);
        }
        catch (EntityNotFoundException)
        {
            return;
        }

        await DeleteAssignedAndInheritedCountingCircles<DomainOfInfluence, DomainOfInfluenceCountingCircle, DomainOfInfluenceHierarchyEntry>(doi);

        foreach (var snapshotDoi in snapshotDois)
        {
            await DeleteAssignedAndInheritedCountingCircles<ContestDomainOfInfluence, ContestDomainOfInfluenceCountingCircle, ContestDomainOfInfluenceHierarchyEntry>(snapshotDoi);
        }
    }

    private async Task UpdateDomainOfInfluenceCountingCircles<TDomainOfInfluence, TDomainOfInfluenceCountingCircle, THierarchyEntry>(
        TDomainOfInfluence currentDoi,
        List<Guid> ccIds)
        where TDomainOfInfluence : BaseDomainOfInfluence<TDomainOfInfluence, TDomainOfInfluenceCountingCircle, THierarchyEntry>
        where TDomainOfInfluenceCountingCircle : BaseDomainOfInfluenceCountingCircle, new()
        where THierarchyEntry : IDomainOfInfluenceHierarchyEntry<TDomainOfInfluence>
    {
        var nonInheritedCcIds = currentDoi.CountingCircles!
            .Where(x => !x.Inherited)
            .Select(x => x.CountingCircleId)
            .ToList();

        var ccIdsToRemove = nonInheritedCcIds.Except(ccIds).ToList();
        var ccIdsToAdd = ccIds.Except(nonInheritedCcIds).ToList();

        var hierarchicalGreaterOrSelfDoiIds = currentDoi.HierarchyEntries!.Select(x => x.ParentDomainOfInfluenceId).ToList();
        hierarchicalGreaterOrSelfDoiIds.Add(currentDoi.Id);

        var hierarchicalLowerOrSelfDoiIds = currentDoi.ParentHierarchyEntries!.Select(x => x.DomainOfInfluenceId).ToList();
        hierarchicalLowerOrSelfDoiIds.Add(currentDoi.Id);

        var dbSet = _dbContext.Set<TDomainOfInfluenceCountingCircle>();
        var existingEntries = await dbSet
            .Where(doiCc => hierarchicalGreaterOrSelfDoiIds.Contains(doiCc.DomainOfInfluenceId)
                    && ccIdsToAdd.Contains(doiCc.CountingCircleId)
                    && doiCc.SourceDomainOfInfluenceId == currentDoi.Id)
            .ToListAsync();

        var newEntries = hierarchicalGreaterOrSelfDoiIds.SelectMany(doiId => ccIdsToAdd
            .Where(ccId => !existingEntries.Any(x => x.CountingCircleId == ccId && x.DomainOfInfluenceId == doiId && x.SourceDomainOfInfluenceId == currentDoi.Id))
            .Select(ccId => new TDomainOfInfluenceCountingCircle
            {
                CountingCircleId = ccId,
                DomainOfInfluenceId = doiId,
                SourceDomainOfInfluenceId = currentDoi.Id,
            }));

        dbSet.AddRange(newEntries);

        await dbSet
            .Where(doiCc => hierarchicalGreaterOrSelfDoiIds.Contains(doiCc.DomainOfInfluenceId) && ccIdsToRemove.Contains(doiCc.CountingCircleId) && hierarchicalLowerOrSelfDoiIds.Contains(doiCc.SourceDomainOfInfluenceId))
            .ExecuteDeleteAsync();
    }

    private async Task DeleteAssignedAndInheritedCountingCircles<TDomainOfInfluence, TDomainOfInfluenceCountingCircle, THierarchyEntry>(
        TDomainOfInfluence currentDoi)
        where TDomainOfInfluence : BaseDomainOfInfluence<TDomainOfInfluence, TDomainOfInfluenceCountingCircle, THierarchyEntry>
        where TDomainOfInfluenceCountingCircle : BaseDomainOfInfluenceCountingCircle, new()
        where THierarchyEntry : IDomainOfInfluenceHierarchyEntry<TDomainOfInfluence>
    {
        var hierarchicalLowerOrSelfDoiIds = currentDoi.ParentHierarchyEntries!.Select(x => x.DomainOfInfluenceId).ToList();
        hierarchicalLowerOrSelfDoiIds.Add(currentDoi.Id);

        await _dbContext.Set<TDomainOfInfluenceCountingCircle>()
            .Where(doiCc => hierarchicalLowerOrSelfDoiIds.Contains(doiCc.SourceDomainOfInfluenceId))
            .ExecuteDeleteAsync();
    }

    private async Task<(DomainOfInfluence Doi, List<ContestDomainOfInfluence> SnapshotDois)> LoadDomainOfInfluences(Guid basisDoiId)
    {
        var doi = await _doiRepo.Query()
            .AsSplitQuery()
            .Include(x => x.CountingCircles)
            .Include(x => x.HierarchyEntries)
            .Include(x => x.ParentHierarchyEntries)
            .FirstOrDefaultAsync(x => x.Id == basisDoiId)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluence), basisDoiId);

        var snapshotDois = await _contestDoiRepo.Query()
            .AsSplitQuery()
            .Include(x => x.CountingCircles)
            .Include(x => x.HierarchyEntries)
            .Include(x => x.ParentHierarchyEntries)
            .Where(x => x.BasisDomainOfInfluenceId == basisDoiId)
            .WhereContestInTestingPhase()
            .ToListAsync();

        return (doi, snapshotDois);
    }
}
