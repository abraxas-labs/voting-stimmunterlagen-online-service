// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class MajorityElectionProcessor : PoliticalBusinessProcessor<MajorityElectionEventData>,
    IEventProcessor<MajorityElectionCreated>,
    IEventProcessor<MajorityElectionUpdated>,
    IEventProcessor<MajorityElectionActiveStateUpdated>,
    IEventProcessor<MajorityElectionDeleted>,
    IEventProcessor<MajorityElectionToNewContestMoved>
{
    public MajorityElectionProcessor(
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
        EventProcessorScope scope)
        : base(mapper, politicalBusinessRepo, politicalBusinessTranslationRepo, politicalBusinessPermissionBuilder, doiVotingCardLayoutBuilder, doiVotingCardConfigurationBuilder, printJobBuilder, doiRepo, stepsBuilder, attachmentBuilder, voterListBuilder, scope)
    {
    }

    public Task Process(MajorityElectionCreated eventData)
        => ProcessCreated(eventData.MajorityElection.Id, eventData.MajorityElection);

    public Task Process(MajorityElectionUpdated eventData)
        => ProcessUpdated(eventData.MajorityElection.Id, eventData.MajorityElection);

    public Task Process(MajorityElectionDeleted eventData)
        => ProcessDeleted(eventData.MajorityElectionId);

    public Task Process(MajorityElectionActiveStateUpdated eventData)
        => ProcessActiveStateUpdated(eventData.MajorityElectionId, eventData.Active);

    public Task Process(MajorityElectionToNewContestMoved eventData)
        => ProcessToNewContestMoved(eventData.MajorityElectionId, eventData.NewContestId);
}
