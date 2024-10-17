// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class DomainOfInfluenceCountingCircleBuilder
{
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IDbRepository<DomainOfInfluenceCountingCircle> _doiCcRepo;
    private readonly IDbRepository<ContestDomainOfInfluenceCountingCircle> _contestDoiCcRepo;

    public DomainOfInfluenceCountingCircleBuilder(
        IDbRepository<Contest> contestRepo,
        IDbRepository<DomainOfInfluenceCountingCircle> doiCcRepo,
        IDbRepository<ContestDomainOfInfluenceCountingCircle> contestDoiCcRepo)
    {
        _contestRepo = contestRepo;
        _doiCcRepo = doiCcRepo;
        _contestDoiCcRepo = contestDoiCcRepo;
    }

    internal async Task UpdateDomainOfInfluenceCountingCircles(Guid doiBasisId, List<Guid> ccBasisIds)
    {
        var existingDoiCcs = await _doiCcRepo.Query()
            .Where(x => x.DomainOfInfluenceId == doiBasisId)
            .ToListAsync();

        var toRemove = existingDoiCcs
            .Where(x => !ccBasisIds.Contains(x.CountingCircleId))
            .ToList();
        await _doiCcRepo.DeleteRangeByKey(toRemove.Select(x => x.Id));

        var toAdd = ccBasisIds
            .Where(x => existingDoiCcs.All(e => e.CountingCircleId != x))
            .Select(ccId => new DomainOfInfluenceCountingCircle
            {
                DomainOfInfluenceId = doiBasisId,
                CountingCircleId = ccId,
            })
            .ToList();
        await _doiCcRepo.CreateRange(toAdd);
    }

    internal async Task UpdateContestDomainOfInfluenceCountingCircles(Guid doiBasisId, List<Guid> ccBasisIds)
    {
        var contests = await _contestRepo.Query()
            .WhereInTestingPhase()
            .ToListAsync();

        var ungroupedExistingDoiCcs = await _contestDoiCcRepo.Query()
            .Include(x => x.DomainOfInfluence)
            .Where(x => x.DomainOfInfluence!.BasisDomainOfInfluenceId == doiBasisId)
            .ToListAsync();
        var existingDoiCcsByContestId = ungroupedExistingDoiCcs
            .GroupBy(x => x.DomainOfInfluence!.ContestId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var toRemove = new List<Guid>();
        var toAdd = new List<ContestDomainOfInfluenceCountingCircle>();
        foreach (var contest in contests)
        {
            var doiId = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(contest.Id, doiBasisId);
            var ccIds = ccBasisIds.ConvertAll(x => StimmunterlagenUuidV5.BuildContestCountingCircle(contest.Id, x));

            var existingDoiCcs = existingDoiCcsByContestId.GetValueOrDefault(contest.Id, new List<ContestDomainOfInfluenceCountingCircle>());
            var doiCcsToRemove = existingDoiCcs
                .Where(x => !ccIds.Contains(x.CountingCircleId))
                .Select(x => x.Id);
            toRemove.AddRange(doiCcsToRemove);

            var doiCcsToAdd = ccIds
                .Where(x => existingDoiCcs.All(e => e.CountingCircleId != x))
                .Select(ccId => new ContestDomainOfInfluenceCountingCircle
                {
                    DomainOfInfluenceId = doiId,
                    CountingCircleId = ccId,
                });
            toAdd.AddRange(doiCcsToAdd);
        }

        await _contestDoiCcRepo.DeleteRangeByKey(toRemove);
        await _contestDoiCcRepo.CreateRange(toAdd);
    }
}
