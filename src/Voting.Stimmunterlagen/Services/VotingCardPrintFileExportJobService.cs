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

public class VotingCardPrintFileExportJobService : Proto.V1.VotingCardPrintFileExportJobService.VotingCardPrintFileExportJobServiceBase
{
    private readonly VotingCardPrintFileExportJobManager _manager;
    private readonly IMapper _mapper;
    private readonly AppContext _appContext;

    public VotingCardPrintFileExportJobService(VotingCardPrintFileExportJobManager manager, IMapper mapper, AppContext appContext)
    {
        _manager = manager;
        _mapper = mapper;
        _appContext = appContext;
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<VotingCardPrintFileExportJobs> List(ListVotingCardPrintFileExportJobsRequest request, ServerCallContext context)
    {
        var jobs = await _manager.ListJobs(GuidParser.Parse(request.DomainOfInfluenceId), _appContext.IsPrintJobManagementApp);
        return _mapper.Map<VotingCardPrintFileExportJobs>(jobs);
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> Retry(RetryVotingCardPrintFileExportJobsRequest request, ServerCallContext context)
    {
        await _manager.RetryJobs(GuidParser.Parse(request.DomainOfInfluenceId));
        return ProtobufEmpty.Instance;
    }
}
