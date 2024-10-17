// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class PoliticalAssemblyProcessor :
    IEventProcessor<PoliticalAssemblyCreated>,
    IEventProcessor<PoliticalAssemblyUpdated>,
    IEventProcessor<PoliticalAssemblyDeleted>
{
    private readonly ContestRepo _contestRepo;
    private readonly ContestBuilder _contestBuilder;

    public PoliticalAssemblyProcessor(ContestRepo contestRepo, ContestBuilder contestBuilder)
    {
        _contestRepo = contestRepo;
        _contestBuilder = contestBuilder;
    }

    public Task Process(PoliticalAssemblyCreated eventData)
        => _contestBuilder.CreateOrUpdatePoliticalAssembly(eventData.PoliticalAssembly);

    public Task Process(PoliticalAssemblyUpdated eventData)
        => _contestBuilder.CreateOrUpdatePoliticalAssembly(eventData.PoliticalAssembly);

    public async Task Process(PoliticalAssemblyDeleted eventData)
    {
        var id = GuidParser.Parse(eventData.PoliticalAssemblyId);
        await _contestRepo.DeleteByKeyIfExists(id);
        await _contestRepo.DeleteVoterContestIndexSequence(id);
    }
}
