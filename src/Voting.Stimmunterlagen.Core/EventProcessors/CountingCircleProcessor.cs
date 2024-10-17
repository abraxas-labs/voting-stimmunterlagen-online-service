// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class CountingCircleProcessor :
    IEventProcessor<CountingCircleCreated>,
    IEventProcessor<CountingCircleDeleted>,
    IEventProcessor<CountingCircleUpdated>,
    IEventProcessor<CountingCirclesMergerActivated>
{
    private readonly IMapper _mapper;
    private readonly IDbRepository<CountingCircle> _ccRepo;
    private readonly ContestCountingCircleRepo _contestCcRepo;
    private readonly ContestCountingCircleBuilder _contestCountingCircleBuilder;

    public CountingCircleProcessor(
        IMapper mapper,
        IDbRepository<CountingCircle> ccRepo,
        ContestCountingCircleRepo contestCcRepo,
        ContestCountingCircleBuilder contestCountingCircleBuilder)
    {
        _mapper = mapper;
        _ccRepo = ccRepo;
        _contestCcRepo = contestCcRepo;
        _contestCountingCircleBuilder = contestCountingCircleBuilder;
    }

    public async Task Process(CountingCircleCreated eventData)
    {
        var countingCircle = _mapper.Map<CountingCircle>(eventData.CountingCircle);
        if (await _ccRepo.ExistsByKey(countingCircle.Id))
        {
            await Update(eventData.CountingCircle);
            return;
        }

        await _ccRepo.Create(countingCircle);
        await _contestCountingCircleBuilder.CreateMissingContestDataForCountingCircle(countingCircle.Id);
    }

    public Task Process(CountingCircleUpdated eventData)
        => Update(eventData.CountingCircle);

    public async Task Process(CountingCircleDeleted eventData)
    {
        var id = GuidParser.Parse(eventData.CountingCircleId);
        var contestCcIdsToDelete = await _contestCcRepo.GetIdsOfContestsInTestingPhaseByBaseId(id);
        await _contestCcRepo.DeleteRangeByKey(contestCcIdsToDelete);
        await _ccRepo.DeleteByKeyIfExists(id);
    }

    public async Task Process(CountingCirclesMergerActivated eventData)
    {
        var newCountingCircle = _mapper.Map<CountingCircle>(eventData.Merger.NewCountingCircle);
        var copyFromCcId = GuidParser.Parse(eventData.Merger.CopyFromCountingCircleId);
        var ccIdsToDelete = eventData.Merger.MergedCountingCircleIds.Select(GuidParser.Parse).ToList();

        if (await _ccRepo.ExistsByKey(newCountingCircle.Id))
        {
            await Update(eventData.Merger.NewCountingCircle);
            return;
        }

        // Update basis (non-snapshot) counting circles
        var copyFromCc = await _ccRepo.Query()
            .Include(cc => cc.DomainOfInfluences)
            .FirstOrDefaultAsync(cc => cc.Id == copyFromCcId)
            ?? throw new EntityNotFoundException(nameof(CountingCircle), copyFromCcId);
        newCountingCircle.DomainOfInfluences = copyFromCc.DomainOfInfluences!.Select(doiCc => new DomainOfInfluenceCountingCircle
        {
            DomainOfInfluenceId = doiCc.DomainOfInfluenceId,
        }).ToList();
        await _ccRepo.Create(newCountingCircle);
        await _ccRepo.DeleteRangeByKey(ccIdsToDelete);

        // Create contest counting circles
        await _contestCountingCircleBuilder.CreateMissingContestDataForCountingCircle(newCountingCircle.Id);

        // Remove old contest counting circles
        var snapshotCcIdsToDelete = await _contestCcRepo.Query()
            .WhereContestInTestingPhase()
            .Where(cc => ccIdsToDelete.Contains(cc.BasisCountingCircleId))
            .Select(cc => cc.Id)
            .ToListAsync();
        await _contestCcRepo.DeleteRangeByKey(snapshotCcIdsToDelete);
    }

    private async Task Update(CountingCircleEventData updatedCountingCircle)
    {
        var countingCircleId = GuidParser.Parse(updatedCountingCircle.Id);
        await UpdateCountingCircles(
            countingCircleId,
            doi => _mapper.Map(updatedCountingCircle, doi),
            doi => _mapper.Map(updatedCountingCircle, doi));
    }

    private async Task UpdateCountingCircles(
        Guid id,
        Action<CountingCircle> updateCountingCircle,
        Action<ContestCountingCircle> updateContestCountingCircle)
    {
        var countingCircle = await _ccRepo.GetByKey(id)
            ?? throw new EntityNotFoundException(nameof(CountingCircle), id);
        updateCountingCircle(countingCircle);
        await _ccRepo.Update(countingCircle);

        var contestCcs = await _contestCcRepo.Query()
            .WhereContestInTestingPhase()
            .Where(x => x.BasisCountingCircleId == countingCircle.Id)
            .ToListAsync();

        foreach (var contestCc in contestCcs)
        {
            updateContestCountingCircle(contestCc);
            contestCc.Id = StimmunterlagenUuidV5.BuildContestCountingCircle(contestCc.ContestId, countingCircle.Id);
        }

        await _contestCcRepo.UpdateRange(contestCcs);
    }
}
