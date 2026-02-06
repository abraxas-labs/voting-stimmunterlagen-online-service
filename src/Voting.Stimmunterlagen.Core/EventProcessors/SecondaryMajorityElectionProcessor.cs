// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class SecondaryMajorityElectionProcessor : PoliticalBusinessProcessor<SecondaryMajorityElectionEventData>,
    IEventProcessor<SecondaryMajorityElectionCreated>,
    IEventProcessor<SecondaryMajorityElectionUpdated>,
    IEventProcessor<SecondaryMajorityElectionDeleted>,
    IEventProcessor<SecondaryMajorityElectionActiveStateUpdated>,
    IEventProcessor<SecondaryMajorityElectionEVotingApprovalUpdated>
{
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;
    private readonly IMapper _mapper;
    private readonly PoliticalBusinessPermissionBuilder _politicalBusinessPermissionBuilder;
    private readonly DomainOfInfluenceVotingCardLayoutBuilder _doiVotingCardLayoutBuilder;
    private readonly DomainOfInfluenceVotingCardConfigurationBuilder _doiVotingCardConfigurationBuilder;

    public SecondaryMajorityElectionProcessor(
        IDbRepository<PoliticalBusiness> politicalBusinessRepo,
        IDbRepository<PoliticalBusinessTranslation> politicalBusinessTranslationRepo,
        IMapper mapper,
        PoliticalBusinessPermissionBuilder politicalBusinessPermissionBuilder,
        DomainOfInfluenceVotingCardLayoutBuilder doiVotingCardLayoutBuilder,
        PrintJobBuilder printJobBuilder,
        DomainOfInfluenceVotingCardConfigurationBuilder doiVotingCardConfigurationBuilder,
        ContestDomainOfInfluenceRepo doiRepo,
        StepsBuilder stepsBuilder,
        AttachmentBuilder attachmentBuilder,
        VoterListBuilder voterListBuilder,
        EventProcessorScope scope)
        : base(mapper, politicalBusinessRepo, politicalBusinessTranslationRepo, politicalBusinessPermissionBuilder, doiVotingCardLayoutBuilder, doiVotingCardConfigurationBuilder, printJobBuilder, doiRepo, stepsBuilder, attachmentBuilder, voterListBuilder, scope)
    {
        _politicalBusinessRepo = politicalBusinessRepo;
        _mapper = mapper;
        _politicalBusinessPermissionBuilder = politicalBusinessPermissionBuilder;
        _doiVotingCardLayoutBuilder = doiVotingCardLayoutBuilder;
        _doiVotingCardConfigurationBuilder = doiVotingCardConfigurationBuilder;
    }

    public async Task Process(SecondaryMajorityElectionCreated eventData)
    {
        var id = GuidParser.Parse(eventData.SecondaryMajorityElection.Id);
        var primaryElectionId = GuidParser.Parse(eventData.SecondaryMajorityElection.PrimaryMajorityElectionId);
        var primaryElection = await _politicalBusinessRepo.Query()
            .Where(x => x.Id == primaryElectionId && x.PoliticalBusinessType == PoliticalBusinessType.MajorityElection)
            .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), primaryElectionId);

        var existingModel = await _politicalBusinessRepo.GetByKey(id);
        if (existingModel == null)
        {
            var model = _mapper.Map<PoliticalBusiness>(eventData.SecondaryMajorityElection);
            model.ContestId = primaryElection.ContestId;
            model.DomainOfInfluenceId = primaryElection.DomainOfInfluenceId;
            await _politicalBusinessRepo.Create(model);
        }
        else
        {
            _mapper.Map(eventData.SecondaryMajorityElection, existingModel);
            existingModel.ContestId = primaryElection.ContestId;
            existingModel.DomainOfInfluenceId = primaryElection.DomainOfInfluenceId;
            await DeletePoliticalBusinessTranslations(id);
            await _politicalBusinessRepo.Update(existingModel);
        }

        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusiness(id);
        await _doiVotingCardLayoutBuilder.SyncForContest(primaryElection.ContestId);
        await _doiVotingCardConfigurationBuilder.SyncForContest(primaryElection.ContestId);
    }

    public async Task Process(SecondaryMajorityElectionUpdated eventData)
    {
        var primaryElectionId = GuidParser.Parse(eventData.SecondaryMajorityElection.PrimaryMajorityElectionId);
        var primaryElection = await _politicalBusinessRepo.Query()
            .Where(x => x.Id == primaryElectionId && x.PoliticalBusinessType == PoliticalBusinessType.MajorityElection)
            .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(nameof(PoliticalBusiness), primaryElectionId);

        var model = _mapper.Map<PoliticalBusiness>(eventData.SecondaryMajorityElection);
        model.ContestId = primaryElection.ContestId;
        model.DomainOfInfluenceId = primaryElection.DomainOfInfluenceId;

        await DeletePoliticalBusinessTranslations(model.Id);
        await _politicalBusinessRepo.Update(model);
        await _politicalBusinessPermissionBuilder.UpdatePermissionsForPoliticalBusiness(model.Id);
        await _doiVotingCardLayoutBuilder.SyncForContest(primaryElection.ContestId);
        await _doiVotingCardConfigurationBuilder.SyncForContest(primaryElection.ContestId);
    }

    public Task Process(SecondaryMajorityElectionDeleted eventData)
        => ProcessDeleted(eventData.SecondaryMajorityElectionId);

    public Task Process(SecondaryMajorityElectionActiveStateUpdated eventData)
        => ProcessActiveStateUpdated(
            eventData.SecondaryMajorityElectionId,
            eventData.Active);

    public Task Process(SecondaryMajorityElectionEVotingApprovalUpdated eventData)
        => ProcessEVotingApprovalUpdated(
            eventData.SecondaryMajorityElectionId,
            eventData.Approved);
}
