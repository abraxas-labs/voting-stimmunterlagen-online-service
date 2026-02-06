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

public class DomainOfInfluenceProcessor :
    IEventProcessor<DomainOfInfluenceCreated>,
    IEventProcessor<DomainOfInfluenceUpdated>,
    IEventProcessor<DomainOfInfluenceDeleted>,
    IEventProcessor<DomainOfInfluenceCountingCircleEntriesUpdated>,
    IEventProcessor<DomainOfInfluenceLogoUpdated>,
    IEventProcessor<DomainOfInfluenceLogoDeleted>,
    IEventProcessor<DomainOfInfluenceVotingCardDataUpdated>
{
    private readonly IMapper _mapper;
    private readonly DomainOfInfluenceRepo _doiRepo;
    private readonly ContestDomainOfInfluenceRepo _contestDoiRepo;
    private readonly IDbRepository<DomainOfInfluenceHierarchyEntry> _doiHierarchyRepo;
    private readonly PoliticalBusinessPermissionBuilder _politicalBusinessPermissionBuilder;
    private readonly ContestDomainOfInfluenceBuilder _doiBuilder;
    private readonly StepsBuilder _stepsBuilder;
    private readonly DomainOfInfluenceVotingCardLayoutBuilder _doiVotingCardLayoutBuilder;
    private readonly DomainOfInfluenceVotingCardConfigurationBuilder _doiVotingCardConfigurationBuilder;
    private readonly PrintJobBuilder _printJobBuilder;
    private readonly AttachmentBuilder _attachmentBuilder;
    private readonly DomainOfInfluenceCountingCircleBuilder _doiCcBuilder;
    private readonly DomainOfInfluenceCantonDefaultsBuilder _domainOfInfluenceCantonDefaultsBuilder;

    public DomainOfInfluenceProcessor(
        IMapper mapper,
        DomainOfInfluenceRepo doiRepo,
        IDbRepository<DomainOfInfluenceHierarchyEntry> doiHierarchyRepo,
        ContestDomainOfInfluenceBuilder doiBuilder,
        PoliticalBusinessPermissionBuilder politicalBusinessPermissionBuilder,
        StepsBuilder stepsBuilder,
        ContestDomainOfInfluenceRepo contestDoiRepo,
        DomainOfInfluenceVotingCardLayoutBuilder doiVotingCardLayoutBuilder,
        DomainOfInfluenceVotingCardConfigurationBuilder doiVotingCardConfigurationBuilder,
        PrintJobBuilder printJobBuilder,
        AttachmentBuilder attachmentBuilder,
        DomainOfInfluenceCountingCircleBuilder doiCcBuilder,
        DomainOfInfluenceCantonDefaultsBuilder domainOfInfluenceCantonDefaultsBuilder)
    {
        _mapper = mapper;
        _doiRepo = doiRepo;
        _doiHierarchyRepo = doiHierarchyRepo;
        _doiBuilder = doiBuilder;
        _politicalBusinessPermissionBuilder = politicalBusinessPermissionBuilder;
        _stepsBuilder = stepsBuilder;
        _contestDoiRepo = contestDoiRepo;
        _doiVotingCardLayoutBuilder = doiVotingCardLayoutBuilder;
        _doiVotingCardConfigurationBuilder = doiVotingCardConfigurationBuilder;
        _printJobBuilder = printJobBuilder;
        _attachmentBuilder = attachmentBuilder;
        _doiCcBuilder = doiCcBuilder;
        _domainOfInfluenceCantonDefaultsBuilder = domainOfInfluenceCantonDefaultsBuilder;
    }

    public async Task Process(DomainOfInfluenceCreated eventData)
    {
        var doi = _mapper.Map<DomainOfInfluence>(eventData.DomainOfInfluence);
        if (await _doiRepo.ExistsByKey(doi.Id))
        {
            await Update(eventData.DomainOfInfluence);
            return;
        }

        doi.RootId = doi.ParentId.HasValue
            ? await _doiRepo.Query().Where(x => x.Id == doi.ParentId.Value).Select(x => x.RootId).SingleAsync()
            : doi.Id;
        await _doiRepo.Create(doi);
        await _domainOfInfluenceCantonDefaultsBuilder.Update(doi);
        await BuildHierarchyEntries(doi.Id);
        await _doiBuilder.CreateMissingContestDataForDomainOfInfluence(doi.Id);
        await SyncForDomainOfInfluence(doi.Id);
    }

    public Task Process(DomainOfInfluenceUpdated eventData) => Update(eventData.DomainOfInfluence);

    public async Task Process(DomainOfInfluenceDeleted eventData)
    {
        var id = GuidParser.Parse(eventData.DomainOfInfluenceId);
        var contestDoiIdsToDelete = await _contestDoiRepo.GetIdsOfContestsInTestingPhaseByBaseId(id);
        await _contestDoiRepo.DeleteRangeByKey(contestDoiIdsToDelete);
        await _doiRepo.DeleteByKeyIfExists(id);
    }

    public async Task Process(DomainOfInfluenceCountingCircleEntriesUpdated eventData)
    {
        var domainOfInfluenceId = GuidParser.Parse(eventData.DomainOfInfluenceCountingCircleEntries.Id);
        var countingCircleIds = eventData.DomainOfInfluenceCountingCircleEntries.CountingCircleIds
            .Select(GuidParser.Parse)
            .ToList();

        await _doiCcBuilder.UpdateDomainOfInfluenceCountingCircles(domainOfInfluenceId, countingCircleIds);
        await _doiCcBuilder.UpdateContestDomainOfInfluenceCountingCircles(domainOfInfluenceId, countingCircleIds);
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusinessesInTestingPhase();
    }

    public Task Process(DomainOfInfluenceLogoUpdated eventData)
        => UpdateDois(
            GuidParser.Parse(eventData.DomainOfInfluenceId),
            doi => doi.LogoRef = eventData.LogoRef,
            doi => doi.LogoRef = eventData.LogoRef);

    public Task Process(DomainOfInfluenceLogoDeleted eventData)
        => UpdateDois(
            GuidParser.Parse(eventData.DomainOfInfluenceId),
            doi => doi.LogoRef = null,
            doi => doi.LogoRef = null);

    public async Task Process(DomainOfInfluenceVotingCardDataUpdated eventData)
    {
        var doiId = GuidParser.Parse(eventData.DomainOfInfluenceId);
        await UpdateDois(
            doiId,
            doi => _mapper.Map(eventData, doi),
            doi => _mapper.Map(eventData, doi));
        await SyncForDomainOfInfluence(doiId);
    }

    private async Task Update(DomainOfInfluenceEventData updatedDomainOfInfluence)
    {
        var doiId = GuidParser.Parse(updatedDomainOfInfluence.Id);
        await UpdateDois(
            doiId,
            doi => _mapper.Map(updatedDomainOfInfluence, doi),
            doi => _mapper.Map(updatedDomainOfInfluence, doi));
        await SyncForDomainOfInfluence(doiId);
    }

    private async Task UpdateDois(
        Guid id,
        Action<DomainOfInfluence> updateDoi,
        Action<ContestDomainOfInfluence> updateContestDoi)
    {
        var doi = await _doiRepo.GetByKey(id)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluence), id);
        var prevCanton = doi.Canton;
        var prevRootId = doi.RootId;
        updateDoi(doi);
        doi.RootId = prevRootId;

        var updateCantonSettings = false;
        if (doi.Canton == DomainOfInfluenceCanton.Unspecified)
        {
            doi.Canton = prevCanton;
        }
        else if (prevCanton != doi.Canton)
        {
            updateCantonSettings = true;
        }

        await _doiRepo.Update(doi);

        var contestDois = await _contestDoiRepo.Query()
            .WhereContestInTestingPhase()
            .Where(x => x.BasisDomainOfInfluenceId == doi.Id)
            .ToListAsync();

        foreach (var contestDoi in contestDois)
        {
            var prevParentId = contestDoi.ParentId; // parent is immutable
            prevRootId = contestDoi.RootId; // since parent is immutable, root is immutable too
            var prevRole = contestDoi.Role; // since parent is immutable, the role cannot change
            updateContestDoi(contestDoi);

            contestDoi.Id = StimmunterlagenUuidV5.BuildContestDomainOfInfluence(contestDoi.ContestId, doi.Id);
            contestDoi.ParentId = prevParentId;
            contestDoi.RootId = prevRootId;
            contestDoi.Role = prevRole;

            if (contestDoi.Canton == DomainOfInfluenceCanton.Unspecified)
            {
                contestDoi.Canton = prevCanton;
            }
        }

        await _contestDoiRepo.UpdateRange(contestDois);

        if (updateCantonSettings)
        {
            await _domainOfInfluenceCantonDefaultsBuilder.UpdateHierarchicalForRoot(doi);
        }
    }

    private async Task SyncForDomainOfInfluence(Guid id)
    {
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusinessesInTestingPhase();
        await _stepsBuilder.SyncStepsForBasisDomainOfInfluence(id);
        await _doiVotingCardLayoutBuilder.SyncForBasisDomainOfInfluence(id);
        await _doiVotingCardConfigurationBuilder.SyncForBasisDomainOfInfluence(id);
        await _printJobBuilder.SyncForBasisDomainOfInfluence(id);
        await _attachmentBuilder.SyncForBasisDomainOfInfluence(id);
    }

    private async Task BuildHierarchyEntries(Guid id)
    {
        var ids = await _doiRepo.GetHierarchicalParentIds(id);
        var newEntries = ids.Select(x => new DomainOfInfluenceHierarchyEntry
        {
            DomainOfInfluenceId = id,
            ParentDomainOfInfluenceId = x,
        });
        await _doiHierarchyRepo.CreateRange(newEntries);
    }
}
