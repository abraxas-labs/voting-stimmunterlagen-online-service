// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class DomainOfInfluenceService : Proto.V1.DomainOfInfluenceService.DomainOfInfluenceServiceBase
{
    private readonly DomainOfInfluenceManager _domainOfInfluenceManager;
    private readonly IMapper _mapper;

    public DomainOfInfluenceService(DomainOfInfluenceManager domainOfInfluenceManager, IMapper mapper)
    {
        _domainOfInfluenceManager = domainOfInfluenceManager;
        _mapper = mapper;
    }

    public override async Task<DomainOfInfluence> Get(IdValueRequest request, ServerCallContext context)
    {
        var doi = await _domainOfInfluenceManager.Get(request.GetId());
        return _mapper.Map<DomainOfInfluence>(doi);
    }

    public override async Task<DomainOfInfluences> ListManagedByCurrentTenant(ListDomainOfInfluencesRequest request, ServerCallContext context)
    {
        var dois = await _domainOfInfluenceManager.ListManagedByCurrentTenant(GuidParser.Parse(request.ContestId));
        return _mapper.Map<DomainOfInfluences>(dois);
    }

    public override async Task<EVotingDomainOfInfluenceEntries> ListEVoting(ListEVotingDomainOfInfluencesRequest request, ServerCallContext context)
    {
        var entries = await _domainOfInfluenceManager.ListEVoting(GuidParser.Parse(request.ContestId));
        return _mapper.Map<EVotingDomainOfInfluenceEntries>(entries);
    }

    public override async Task<DomainOfInfluences> ListChildren(ListDomainOfInfluenceChildrenRequest request, ServerCallContext context)
    {
        var dois = await _domainOfInfluenceManager.ListChildren(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<DomainOfInfluences>(dois);
    }

    public override async Task<Empty> UpdateSettings(UpdateDomainOfInfluenceSettingsRequest request, ServerCallContext context)
    {
        await _domainOfInfluenceManager.UpdateSettings(GuidParser.Parse(request.DomainOfInfluenceId), request.AllowManualVoterListUpload);
        return ProtobufEmpty.Instance;
    }
}
