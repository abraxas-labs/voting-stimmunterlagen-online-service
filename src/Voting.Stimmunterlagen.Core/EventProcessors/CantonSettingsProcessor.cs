// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class CantonSettingsProcessor :
    IEventProcessor<CantonSettingsCreated>,
    IEventProcessor<CantonSettingsUpdated>
{
    private readonly IDbRepository<CantonSettings> _repo;
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceCantonDefaultsBuilder _builder;

    public CantonSettingsProcessor(
        IDbRepository<CantonSettings> repo,
        IMapper mapper,
        DomainOfInfluenceCantonDefaultsBuilder builder)
    {
        _repo = repo;
        _mapper = mapper;
        _builder = builder;
    }

    public Task Process(CantonSettingsCreated eventData) => CreateOrUpdate(eventData.CantonSettings);

    public Task Process(CantonSettingsUpdated eventData) => CreateOrUpdate(eventData.CantonSettings);

    private async Task CreateOrUpdate(CantonSettingsEventData cantonSettings)
    {
        var id = GuidParser.Parse(cantonSettings.Id);
        var existing = await _repo.GetByKey(id);
        if (existing != null)
        {
            _mapper.Map(cantonSettings, existing);
            await _repo.Update(existing);
            await _builder.UpdateHierarchical(existing);
            return;
        }

        var model = _mapper.Map<CantonSettings>(cantonSettings);
        await _repo.Create(model);
        await _builder.UpdateHierarchical(model);
    }
}
