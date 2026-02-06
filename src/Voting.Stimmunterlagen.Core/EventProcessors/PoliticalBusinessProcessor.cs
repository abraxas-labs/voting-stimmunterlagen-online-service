// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public abstract class PoliticalBusinessProcessor<TPoliticalBusinessBasisProto>
    where TPoliticalBusinessBasisProto : IMessage<TPoliticalBusinessBasisProto>
{
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;
    private readonly IDbRepository<PoliticalBusinessTranslation> _politicalBusinessTranslationRepo;
    private readonly ContestDomainOfInfluenceRepo _doiRepo;
    private readonly IMapper _mapper;
    private readonly PoliticalBusinessPermissionBuilder _politicalBusinessPermissionBuilder;
    private readonly DomainOfInfluenceVotingCardLayoutBuilder _doiVotingCardLayoutBuilder;
    private readonly DomainOfInfluenceVotingCardConfigurationBuilder _doiVotingCardConfigurationBuilder;
    private readonly PrintJobBuilder _printJobBuilder;
    private readonly StepsBuilder _stepsBuilder;
    private readonly AttachmentBuilder _attachmentBuilder;
    private readonly VoterListBuilder _voterListBuilder;
    private readonly EventProcessorScope _eventProcessorScope;

    protected PoliticalBusinessProcessor(
        IMapper mapper,
        IDbRepository<PoliticalBusiness> politicalBusinessRepo,
        IDbRepository<PoliticalBusinessTranslation> politicalBusinessTranslationRepo,
        PoliticalBusinessPermissionBuilder politicalBusinessPermissionBuilder,
        DomainOfInfluenceVotingCardLayoutBuilder doiVotingCardLayoutBuilder,
        DomainOfInfluenceVotingCardConfigurationBuilder doiVotingCardConfigurationBuilder,
        PrintJobBuilder printJobBuilder,
        ContestDomainOfInfluenceRepo doiRepo,
        StepsBuilder stepsBuilder,
        AttachmentBuilder attachmentBuilder,
        VoterListBuilder voterListBuilder,
        EventProcessorScope eventProcessorScope)
    {
        _politicalBusinessRepo = politicalBusinessRepo;
        _politicalBusinessTranslationRepo = politicalBusinessTranslationRepo;
        _politicalBusinessPermissionBuilder = politicalBusinessPermissionBuilder;
        _doiRepo = doiRepo;
        _stepsBuilder = stepsBuilder;
        _eventProcessorScope = eventProcessorScope;
        _doiVotingCardLayoutBuilder = doiVotingCardLayoutBuilder;
        _doiVotingCardConfigurationBuilder = doiVotingCardConfigurationBuilder;
        _printJobBuilder = printJobBuilder;
        _attachmentBuilder = attachmentBuilder;
        _voterListBuilder = voterListBuilder;
        _mapper = mapper;
    }

    protected async Task ProcessCreated(string id, TPoliticalBusinessBasisProto politicalBusinessEventData)
    {
        var guid = GuidParser.Parse(id);
        var model = await _politicalBusinessRepo.GetByKey(guid);

        if (model != null)
        {
            await Update(model, politicalBusinessEventData);
            return;
        }

        model = _mapper.Map<PoliticalBusiness>(politicalBusinessEventData);
        model.DomainOfInfluenceId = await _doiRepo.GetIdForContest(model.ContestId, model.DomainOfInfluenceId);
        await _politicalBusinessRepo.Create(model);
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusiness(guid);
        await SyncForContest(model.ContestId);
    }

    protected async Task ProcessUpdated(string id, TPoliticalBusinessBasisProto politicalBusinessEventData)
    {
        var guid = GuidParser.Parse(id);
        var model = await _politicalBusinessRepo.GetByKey(guid)
            ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), guid);
        await Update(model, politicalBusinessEventData);
    }

    protected async Task ProcessDeleted(string id)
    {
        var politicalBusiness = await _politicalBusinessRepo.GetByKey(GuidParser.Parse(id));
        if (politicalBusiness == null)
        {
            return;
        }

        await _politicalBusinessRepo.DeleteByKey(politicalBusiness.Id);
        await _attachmentBuilder.CleanUp(new[] { politicalBusiness.ContestId });
        await _voterListBuilder.CleanUp(new[] { politicalBusiness.ContestId });
        await SyncForContest(politicalBusiness.ContestId);
    }

    protected async Task ProcessActiveStateUpdated(string id, bool active)
    {
        var guid = GuidParser.Parse(id);
        var existingModel = await _politicalBusinessRepo.GetByKey(guid)
                            ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), guid);

        existingModel.Active = active;
        await _politicalBusinessRepo.Update(existingModel);
    }

    protected async Task ProcessEVotingApprovalUpdated(string id, bool approved)
    {
        var guid = GuidParser.Parse(id);
        var existingModel = await _politicalBusinessRepo.GetByKey(guid)
                            ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), guid);

        existingModel.EVotingApproved = approved;
        await _politicalBusinessRepo.Update(existingModel);
    }

    protected async Task DeletePoliticalBusinessTranslations(Guid politicalBusinessId)
    {
        var translationIds = await _politicalBusinessTranslationRepo.Query()
            .IgnoreQueryFilters() // do not filter translations
            .Where(x => x.PoliticalBusinessId == politicalBusinessId)
            .Select(x => x.Id)
            .ToListAsync();
        await _politicalBusinessTranslationRepo.DeleteRangeByKey(translationIds);
    }

    protected async Task ProcessToNewContestMoved(string id, string newContestId)
    {
        var guid = GuidParser.Parse(id);
        var newContestGuid = GuidParser.Parse(newContestId);

        var existingModel = await _politicalBusinessRepo.Query()
            .Include(pb => pb.DomainOfInfluence)
            .FirstOrDefaultAsync(pb => pb.Id == guid)
            ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), guid);

        existingModel.ContestId = GuidParser.Parse(newContestId);
        existingModel.DomainOfInfluenceId = await _doiRepo.GetIdForContest(newContestGuid, existingModel.DomainOfInfluence!.BasisDomainOfInfluenceId);
        existingModel.DomainOfInfluence = null;

        await _politicalBusinessRepo.Update(existingModel);
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusinessesInTestingPhase();

        if (!_eventProcessorScope.IsInReplay)
        {
            await SyncForContest(existingModel.ContestId);
        }
    }

    private async Task Update(PoliticalBusiness existingData, TPoliticalBusinessBasisProto updatedData)
    {
        _mapper.Map(updatedData, existingData);

        existingData.DomainOfInfluenceId = await _doiRepo.GetIdForContest(existingData.ContestId, existingData.DomainOfInfluenceId);

        await DeletePoliticalBusinessTranslations(existingData.Id);
        await _politicalBusinessRepo.Update(existingData);
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusiness(existingData.Id);
        if (!_eventProcessorScope.IsInReplay)
        {
            await SyncForContest(existingData.ContestId);
        }
    }

    private async Task SyncForContest(Guid contestId)
    {
        await _stepsBuilder.SyncStepsForContest(contestId);
        await _doiVotingCardLayoutBuilder.SyncForContest(contestId);
        await _doiVotingCardConfigurationBuilder.SyncForContest(contestId);
        await _printJobBuilder.SyncForContest(contestId);
    }
}
