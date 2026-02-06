// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ProportionalElectionProcessor : PoliticalBusinessProcessor<ProportionalElectionEventData>,
    IEventProcessor<ProportionalElectionCreated>,
    IEventProcessor<ProportionalElectionUpdated>,
    IEventProcessor<ProportionalElectionActiveStateUpdated>,
    IEventProcessor<ProportionalElectionEVotingApprovalUpdated>,
    IEventProcessor<ProportionalElectionDeleted>,
    IEventProcessor<ProportionalElectionToNewContestMoved>
{
    public ProportionalElectionProcessor(
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

    public Task Process(ProportionalElectionCreated eventData)
        => ProcessCreated(eventData.ProportionalElection.Id, eventData.ProportionalElection);

    public Task Process(ProportionalElectionUpdated eventData)
        => ProcessUpdated(eventData.ProportionalElection.Id, eventData.ProportionalElection);

    public Task Process(ProportionalElectionDeleted eventData)
        => ProcessDeleted(eventData.ProportionalElectionId);

    public Task Process(ProportionalElectionActiveStateUpdated eventData)
        => ProcessActiveStateUpdated(eventData.ProportionalElectionId, eventData.Active);

    public Task Process(ProportionalElectionEVotingApprovalUpdated eventData)
        => ProcessEVotingApprovalUpdated(eventData.ProportionalElectionId, eventData.Approved);

    public Task Process(ProportionalElectionToNewContestMoved eventData)
        => ProcessToNewContestMoved(eventData.ProportionalElectionId, eventData.NewContestId);
}
