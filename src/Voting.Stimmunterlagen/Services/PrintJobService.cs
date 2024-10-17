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

public class PrintJobService : Proto.V1.PrintJobService.PrintJobServiceBase
{
    private readonly PrintJobManager _printJobManager;
    private readonly IMapper _mapper;
    private readonly AppContext _appContext;

    public PrintJobService(
        PrintJobManager printJobManager,
        IMapper mapper,
        AppContext appContext)
    {
        _printJobManager = printJobManager;
        _mapper = mapper;
        _appContext = appContext;
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<PrintJob> Get(
        GetPrintJobRequest request,
        ServerCallContext context)
    {
        return _mapper.Map<PrintJob>(await _printJobManager.Get(GuidParser.Parse(request.DomainOfInfluenceId), _appContext.IsPrintJobManagementApp));
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<PrintJobSummaries> ListSummaries(ListPrintJobSummariesRequest request, ServerCallContext context)
    {
        return _mapper.Map<PrintJobSummaries>(await _printJobManager.List(
            GuidParser.Parse(request.ContestId),
            _appContext.IsPrintJobManagementApp,
            request.Query,
            request.State == PrintJobState.Unspecified ? null : _mapper.Map<Data.Models.PrintJobState>(request.State)));
    }

    [AuthorizeElectionAdmin]
    public override async Task<PrintJobs> ListGenerateVotingCardsTriggered(ListPrintJobGenerateVotingCardsTriggeredRequest request, ServerCallContext context)
    {
        return _mapper.Map<PrintJobs>(
            await _printJobManager.ListGenerateVotingCardsTriggered(GuidParser.Parse(request.ContestId)));
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> ResetState(
        ResetPrintJobStateRequest request,
        ServerCallContext context)
    {
        await _printJobManager.ResetState(GuidParser.Parse(request.DomainOfInfluenceId));
        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> SetProcessStarted(
        SetPrintJobProcessStartedRequest request,
        ServerCallContext context)
    {
        await _printJobManager.SetProcessStarted(GuidParser.Parse(request.DomainOfInfluenceId));
        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> SetProcessEnded(
        SetPrintJobProcessEndedRequest request,
        ServerCallContext context)
    {
        await _printJobManager.SetProcessEnded(
            GuidParser.Parse(request.DomainOfInfluenceId),
            request.VotingCardsPrintedAndPackedCount,
            request.VotingCardsShipmentWeight);
        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> SetDone(
        SetPrintJobDoneRequest request,
        ServerCallContext context)
    {
        await _printJobManager.SetDone(
            GuidParser.Parse(request.DomainOfInfluenceId),
            request.Comment);
        return ProtobufEmpty.Instance;
    }
}
