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
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ContestState = Voting.Stimmunterlagen.Data.Models.ContestState;

namespace Voting.Stimmunterlagen.Services;

public class ContestService : Proto.V1.ContestService.ContestServiceBase
{
    private readonly IMapper _mapper;
    private readonly ContestManager _contestManager;
    private readonly AppContext _appContext;

    public ContestService(IMapper mapper, ContestManager contestManager, AppContext appContext)
    {
        _mapper = mapper;
        _contestManager = contestManager;
        _appContext = appContext;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> SetDeadlines(SetContestDeadlinesRequest request, ServerCallContext context)
    {
        await _contestManager.SetDeadlines(
            GuidParser.Parse(request.Id),
            request.PrintingCenterSignUpDeadlineDate.ToDateTime(),
            request.AttachmentDeliveryDeadlineDate.ToDateTime(),
            request.GenerateVotingCardsDeadlineDate.ToDateTime());
        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<Contest> Get(IdValueRequest request, ServerCallContext context)
    {
        var contest = await _contestManager.Get(request.GetId(), _appContext.IsPrintJobManagementApp);
        return _mapper.Map<Contest>(contest);
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<Contests> List(ListContestsRequest request, ServerCallContext context)
    {
        var contests = await _contestManager.List(
            request.States.Cast<ContestState>().ToList(),
            _appContext.IsPrintJobManagementApp);

        return _mapper.Map<Contests>(contests);
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> UpdatePrintingCenterSignUpDeadline(UpdateContestPrintingCenterSignupDeadlineRequest request, ServerCallContext context)
    {
        await _contestManager.UpdatePrintingCenterSignUpDeadline(
            GuidParser.Parse(request.Id),
            request.PrintingCenterSignUpDeadlineDate.ToDateTime(),
            request.GenerateVotingCardsDeadlineDate.ToDateTime(),
            request.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Select(GuidParser.Parse).ToList());
        return ProtobufEmpty.Instance;
    }
}
