// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ContestDomainOfInfluenceBuilder
{
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceRepo _doiRepo;
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly ContestDomainOfInfluenceRepo _contestDoiRepo;
    private readonly DataContext _dbContext;

    public ContestDomainOfInfluenceBuilder(
        IMapper mapper,
        DomainOfInfluenceRepo doiRepo,
        IDbRepository<Contest> contestRepo,
        ContestDomainOfInfluenceRepo contestDoiRepo,
        DataContext dbContext)
    {
        _mapper = mapper;
        _doiRepo = doiRepo;
        _contestRepo = contestRepo;
        _contestDoiRepo = contestDoiRepo;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a ContestDomainOfInfluence "snapshot" and related entities for a new DomainOfInfluence for all contests in the testing phase.
    /// </summary>
    /// <param name="doiBasisId">The VOTING Basis counting circle ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task CreateMissingContestDataForDomainOfInfluence(Guid doiBasisId)
    {
        var contests = await _contestRepo.Query()
            .WhereInTestingPhase()
            .Include(x => x.ContestDomainOfInfluences!).ThenInclude(x => x.HierarchyEntries)
            .ToListAsync();
        await CreateMissingContestData(contests, doiBasisId);
        await UpdateContestDomainOfInfluenceRoles(null, doiBasisId);
    }

    /// <summary>
    /// Creates ContestDomainOfInfluences "snapshots" for a new contest from the current state of the DomainOfInfluences.
    /// </summary>
    /// <param name="contest">The contest.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task CreateMissingDataForContest(Contest contest)
    {
        await CreateMissingContestData(new[] { contest }, null);
        await UpdateContestDomainOfInfluenceRoles(contest.Id, null);
    }

    private async Task CreateMissingContestData(IReadOnlyCollection<Contest> contests, Guid? doiBasisId)
    {
        var idMapByContestId = contests
            .SelectMany(x => x.ContestDomainOfInfluences!)
            .GroupBy(x => x.ContestId)
            .ToDictionary(
                x => x.Key,
                x => x.ToDictionary(y => y.BasisDomainOfInfluenceId, y => y.Id));

        var dois = await _doiRepo.Query()
            .Include(x => x.HierarchyEntries)
            .Where(x => doiBasisId == null || x.Id == doiBasisId)
            .ToListAsync();

        var newDois = new List<ContestDomainOfInfluence>();

        foreach (var contest in contests)
        {
            var idMap = idMapByContestId.GetValueOrDefault(contest.Id, new());
            var newContestDois = dois
                .Where(d => !idMap.ContainsKey(d.Id))
                .Select(_mapper.Map<ContestDomainOfInfluence>)
                .ToList();

            foreach (var doi in newContestDois)
            {
                doi.ContestId = contest.Id;
            }

            RegenerateIds(newContestDois, idMap);
            MapParentAndRootIds(idMap, newContestDois);
            MapHierarchyIds(idMap, newContestDois);

            if (idMap.TryGetValue(contest.DomainOfInfluenceId!.Value, out var id))
            {
                contest.DomainOfInfluenceId = id;
            }

            newDois.AddRange(newContestDois);
        }

        await _contestDoiRepo.CreateRange(newDois);
    }

    private async Task UpdateContestDomainOfInfluenceRoles(Guid? contestId, Guid? doiBasisId)
    {
        var dois = await _contestDoiRepo
            .Query()
            .AsTracking()
            .Include(x => x.Contest)
            .Include(x => x.HierarchyEntries)
            .WhereContestInTestingPhase()
            .Where(x => (doiBasisId == null || x.BasisDomainOfInfluenceId == doiBasisId) &&
                        (contestId == null || x.ContestId == contestId))
            .ToListAsync();

        foreach (var doi in dois)
        {
            doi.Role = doi.Id == doi.Contest!.DomainOfInfluenceId
                ? ContestRole.Manager
                : doi.HierarchyEntries!.Any(x => x.ParentDomainOfInfluenceId == doi.Contest!.DomainOfInfluenceId)
                    ? ContestRole.Attendee
                    : ContestRole.None;
        }

        await _dbContext.SaveChangesAsync();
    }

    private void RegenerateIds(IEnumerable<ContestDomainOfInfluence> allDois, Dictionary<Guid, Guid> idMap)
    {
        foreach (var doi in allDois)
        {
            doi.BasisDomainOfInfluenceId = doi.Id;

            var newId = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(doi.ContestId, doi.Id);
            idMap[doi.Id] = newId;
            doi.Id = newId;
        }
    }

    private void MapHierarchyIds(IReadOnlyDictionary<Guid, Guid> idMap, IEnumerable<ContestDomainOfInfluence> dois)
    {
        foreach (var doiHierarchyEntry in dois.SelectMany(x => x.HierarchyEntries!))
        {
            doiHierarchyEntry.Id = Guid.NewGuid();

            doiHierarchyEntry.DomainOfInfluenceId = idMap[doiHierarchyEntry.DomainOfInfluenceId];
            doiHierarchyEntry.ParentDomainOfInfluenceId = idMap[doiHierarchyEntry.ParentDomainOfInfluenceId];
        }
    }

    private void MapParentAndRootIds(IReadOnlyDictionary<Guid, Guid> idMap, IEnumerable<ContestDomainOfInfluence> dois)
    {
        foreach (var doi in dois)
        {
            if (doi.ParentId.HasValue)
            {
                doi.ParentId = idMap[doi.ParentId.Value];
            }

            doi.RootId = idMap[doi.RootId];
        }
    }
}
