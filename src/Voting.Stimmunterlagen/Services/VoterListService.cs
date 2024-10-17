// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class VoterListService : Proto.V1.VoterListService.VoterListServiceBase
{
    private readonly IMapper _mapper;
    private readonly VoterListManager _manager;

    public VoterListService(IMapper mapper, VoterListManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<VoterLists> List(ListVoterListsRequest request, ServerCallContext context)
    {
        var doiId = GuidParser.Parse(request.DomainOfInfluenceId);
        var lists = await _manager.List(doiId);
        return _mapper.Map<VoterLists>(lists);
    }

    public override async Task<Empty> UpdateLists(UpdateVoterListsRequest request, ServerCallContext context)
    {
        var data = request.VoterLists.Select(vl => new VoterListUpdateData(
            GuidParser.Parse(vl.Id),
            vl.PoliticalBusinessIds.Select(GuidParser.Parse).ToList(),
            vl.SendVotingCardsToDomainOfInfluenceReturnAddress)).ToList();

        await _manager.UpdateLists(data);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> AssignPoliticalBusiness(AssignPoliticalBusinessVoterListRequest request, ServerCallContext context)
    {
        await _manager.AssignPoliticalBusiness(
            GuidParser.Parse(request.Id),
            GuidParser.Parse(request.PoliticalBusinessId));

        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> UnassignPoliticalBusiness(UnassignPoliticalBusinessVoterListRequest request, ServerCallContext context)
    {
        await _manager.UnassignPoliticalBusiness(
            GuidParser.Parse(request.Id),
            GuidParser.Parse(request.PoliticalBusinessId));

        return ProtobufEmpty.Instance;
    }
}
