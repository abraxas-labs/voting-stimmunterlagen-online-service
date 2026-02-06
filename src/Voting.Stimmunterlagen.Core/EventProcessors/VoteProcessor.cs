// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class VoteProcessor : PoliticalBusinessProcessor<VoteEventData>,
    IEventProcessor<VoteCreated>,
    IEventProcessor<VoteUpdated>,
    IEventProcessor<VoteActiveStateUpdated>,
    IEventProcessor<VoteEVotingApprovalUpdated>,
    IEventProcessor<VoteDeleted>,
    IEventProcessor<VoteToNewContestMoved>
{
    public VoteProcessor(
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

    public Task Process(VoteCreated eventData)
        => ProcessCreated(eventData.Vote.Id, eventData.Vote);

    public Task Process(VoteUpdated eventData)
        => ProcessUpdated(eventData.Vote.Id, eventData.Vote);

    public Task Process(VoteDeleted eventData)
        => ProcessDeleted(eventData.VoteId);

    public Task Process(VoteActiveStateUpdated eventData)
        => ProcessActiveStateUpdated(eventData.VoteId, eventData.Active);

    public Task Process(VoteEVotingApprovalUpdated eventData)
        => ProcessEVotingApprovalUpdated(eventData.VoteId, eventData.Approved);

    public Task Process(VoteToNewContestMoved eventData)
        => ProcessToNewContestMoved(eventData.VoteId, eventData.NewContestId);
}
