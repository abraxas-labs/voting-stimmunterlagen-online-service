// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ContestProcessor :
    IEventProcessor<ContestCreated>,
    IEventProcessor<ContestUpdated>,
    IEventProcessor<ContestDeleted>,
    IEventProcessor<ContestTestingPhaseEnded>,
    IEventProcessor<ContestPastLocked>,
    IEventProcessor<ContestPastUnlocked>,
    IEventProcessor<ContestArchived>,
#pragma warning disable CS0612 // contest counting circle options are deprecated
    IEventProcessor<ContestCountingCircleOptionsUpdated>
#pragma warning restore CS0612
{
    private readonly ContestRepo _contestRepo;
    private readonly ContestBuilder _contestBuilder;

    public ContestProcessor(ContestRepo contestRepo, ContestBuilder contestBuilder)
    {
        _contestRepo = contestRepo;
        _contestBuilder = contestBuilder;
    }

    public Task Process(ContestCreated eventData)
        => _contestBuilder.CreateOrUpdateContest(eventData.Contest);

    public Task Process(ContestUpdated eventData)
        => _contestBuilder.CreateOrUpdateContest(eventData.Contest);

    public async Task Process(ContestDeleted eventData)
    {
        var id = GuidParser.Parse(eventData.ContestId);
        await _contestRepo.DeleteByKeyIfExists(id);
        await _contestRepo.DeleteVoterContestIndexSequence(id);
    }

    public Task Process(ContestTestingPhaseEnded eventData) => _contestBuilder.UpdateState(eventData.ContestId, ContestState.Active);

    public Task Process(ContestPastLocked eventData) => _contestBuilder.UpdateState(eventData.ContestId, ContestState.PastLocked);

    public Task Process(ContestPastUnlocked eventData) => _contestBuilder.UpdateState(eventData.ContestId, ContestState.PastUnlocked);

    public async Task Process(ContestArchived eventData)
    {
        await _contestBuilder.UpdateState(eventData.ContestId, ContestState.Archived);
        await _contestRepo.DeleteVoterContestIndexSequence(GuidParser.Parse(eventData.ContestId));
    }

    [Obsolete("contest counting circle options are deprecated")]
    public Task Process(ContestCountingCircleOptionsUpdated eventData) =>
        _contestBuilder.UpdateContestCountingCircleOptions(eventData.ContestId, eventData.Options);
}
