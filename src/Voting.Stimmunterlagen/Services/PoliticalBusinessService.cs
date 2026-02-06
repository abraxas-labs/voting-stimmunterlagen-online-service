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

public class PoliticalBusinessService : Proto.V1.PoliticalBusinessService.PoliticalBusinessServiceBase
{
    private readonly IMapper _mapper;
    private readonly PoliticalBusinessManager _politicalBusinessManager;
    private readonly AppContext _appContext;

    public PoliticalBusinessService(PoliticalBusinessManager politicalBusinessManager, IMapper mapper, AppContext appContext)
    {
        _politicalBusinessManager = politicalBusinessManager;
        _mapper = mapper;
        _appContext = appContext;
    }

    [AuthorizeElectionAdmin]
    public override async Task<PoliticalBusinesses> List(ListPoliticalBusinessesRequest request, ServerCallContext context)
    {
        var businesses = await _politicalBusinessManager.List(
            GuidParser.ParseNullable(request.ContestId),
            GuidParser.ParseNullable(request.DomainOfInfluenceId));
        return _mapper.Map<PoliticalBusinesses>(businesses);
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<PoliticalBusinesses> ListAttachmentAccessible(ListAttachmentAccessiblePoliticalBusinessesRequest request, ServerCallContext context)
    {
        var businesses = await _politicalBusinessManager.ListAttachmentAccessible(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _appContext.IsPrintJobManagementApp);
        return _mapper.Map<PoliticalBusinesses>(businesses);
    }
}
