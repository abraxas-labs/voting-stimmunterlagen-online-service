// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class PoliticalBusinessService : Proto.V1.PoliticalBusinessService.PoliticalBusinessServiceBase
{
    private readonly IMapper _mapper;
    private readonly PoliticalBusinessManager _politicalBusinessManager;

    public PoliticalBusinessService(PoliticalBusinessManager politicalBusinessManager, IMapper mapper)
    {
        _politicalBusinessManager = politicalBusinessManager;
        _mapper = mapper;
    }

    public override async Task<PoliticalBusinesses> List(ListPoliticalBusinessesRequest request, ServerCallContext context)
    {
        var businesses = await _politicalBusinessManager.List(
            GuidParser.ParseNullable(request.ContestId),
            GuidParser.ParseNullable(request.DomainOfInfluenceId));
        return _mapper.Map<PoliticalBusinesses>(businesses);
    }
}
