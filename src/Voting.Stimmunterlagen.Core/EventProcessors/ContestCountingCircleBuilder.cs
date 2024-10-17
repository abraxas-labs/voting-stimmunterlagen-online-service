// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ContestCountingCircleBuilder
{
    private readonly IMapper _mapper;
    private readonly IDbRepository<CountingCircle> _ccRepo;
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly ContestCountingCircleRepo _contestCcRepo;

    public ContestCountingCircleBuilder(
        IMapper mapper,
        IDbRepository<CountingCircle> ccRepo,
        IDbRepository<Contest> contestRepo,
        ContestCountingCircleRepo contestCcRepo)
    {
        _mapper = mapper;
        _ccRepo = ccRepo;
        _contestRepo = contestRepo;
        _contestCcRepo = contestCcRepo;
    }

    /// <summary>
    /// Creates a ContestCountingCircle "snapshot" for a new CountingCircle for all contests in the testing phase.
    /// </summary>
    /// <param name="ccBasisId">The VOTING Basis counting circle ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task CreateMissingContestDataForCountingCircle(Guid ccBasisId)
    {
        var contests = await _contestRepo.Query()
            .WhereInTestingPhase()
            .Include(x => x.ContestCountingCircles!)
            .ToListAsync();
        await CreateMissingContestData(contests, ccBasisId);
    }

    /// <summary>
    /// Creates ContestCountingCircles "snapshots" for a new contest from the current state of the CountingCircles.
    /// </summary>
    /// <param name="contest">The contest.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task CreateMissingDataForContest(Contest contest)
    {
        await CreateMissingContestData(new[] { contest }, null);
    }

    private async Task CreateMissingContestData(IReadOnlyCollection<Contest> contests, Guid? ccBasisId)
    {
        var idMapByContestId = contests
            .SelectMany(x => x.ContestCountingCircles!)
            .GroupBy(x => x.ContestId)
            .ToDictionary(
                x => x.Key,
                x => x.ToDictionary(y => y.BasisCountingCircleId, y => y.Id));

        var countingCircles = await _ccRepo.Query()
            .Include(x => x.DomainOfInfluences)
            .Where(x => ccBasisId == null || x.Id == ccBasisId)
            .ToListAsync();
        var newContestCountingCircles = new List<ContestCountingCircle>();

        foreach (var contest in contests)
        {
            var idMap = idMapByContestId.GetValueOrDefault(contest.Id, new Dictionary<Guid, Guid>());
            var newContestCcs = countingCircles
                .Where(d => !idMap.ContainsKey(d.Id))
                .Select(_mapper.Map<ContestCountingCircle>)
                .ToList();

            foreach (var cc in newContestCcs)
            {
                cc.ContestId = contest.Id;
            }

            RegenerateIds(newContestCcs, idMap);
            newContestCountingCircles.AddRange(newContestCcs);
        }

        await _contestCcRepo.CreateRange(newContestCountingCircles);
    }

    private void RegenerateIds(IEnumerable<ContestCountingCircle> contestCountingCircles, Dictionary<Guid, Guid> idMap)
    {
        foreach (var countingCircle in contestCountingCircles)
        {
            countingCircle.BasisCountingCircleId = countingCircle.Id;

            var newId = StimmunterlagenUuidV5.BuildContestCountingCircle(countingCircle.ContestId, countingCircle.Id);
            idMap[countingCircle.Id] = newId;
            countingCircle.Id = newId;

            foreach (var doiCc in countingCircle.DomainOfInfluences!)
            {
                doiCc.Id = Guid.NewGuid();
                doiCc.DomainOfInfluenceId =
                    StimmunterlagenUuidV5.BuildContestDomainOfInfluence(countingCircle.ContestId, doiCc.DomainOfInfluenceId);
            }
        }
    }
}
