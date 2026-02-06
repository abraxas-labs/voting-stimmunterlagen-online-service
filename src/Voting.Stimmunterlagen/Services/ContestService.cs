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
using Voting.Stimmunterlagen.Proto.V1.Responses;
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
            request.GenerateVotingCardsDeadlineDate.ToDateTime(),
            request.ElectoralRegisterEVotingFromDate?.ToDateTime());
        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<GetPreviewCommunalContestDeadlinesResponse> GetPreviewCommunalDeadlines(GetPreviewCommunalDeadlinesRequest request, ServerCallContext context)
    {
        var result = await _contestManager.GetPreviewCommunalDeadlines(GuidParser.Parse(request.Id), request.DeliveryToPostDeadlineDate.ToDateTime());
        return _mapper.Map<GetPreviewCommunalContestDeadlinesResponse>(result);
    }

    [AuthorizeElectionAdmin]
    public override async Task<SetCommunalContestDeadlinesResponse> SetCommunalDeadlines(SetCommunalContestDeadlinesRequest request, ServerCallContext context)
    {
        var result = await _contestManager.SetCommunalDeadlines(GuidParser.Parse(request.Id), request.DeliveryToPostDeadlineDate.ToDateTime());
        return _mapper.Map<SetCommunalContestDeadlinesResponse>(result);
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

        var contestSummaries = contests.Select(contest => CreateContestSummary(contest)).ToList();

        return _mapper.Map<Contests>(contestSummaries);
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> ResetGenerateVotingCardsAndUpdateContestDeadlines(ResetGenerateVotingCardsAndUpdateContestDeadlinesRequest request, ServerCallContext context)
    {
        await _contestManager.ResetGenerateVotingCardsAndUpdateContestDeadlines(
            GuidParser.Parse(request.Id),
            request.PrintingCenterSignUpDeadlineDate.ToDateTime(),
            request.GenerateVotingCardsDeadlineDate.ToDateTime(),
            request.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Select(GuidParser.Parse).ToList());
        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> ResetGenerateVotingCardsAndUpdateCommunalContestDeadlines(ResetGenerateVotingCardsAndUpdateCommunalContestDeadlinesRequest request, ServerCallContext context)
    {
        await _contestManager.ResetGenerateVotingCardsAndUpdateCommunalContestDeadlines(
            GuidParser.Parse(request.Id),
            request.PrintingCenterSignUpDeadlineDate.ToDateTime(),
            request.AttachmentDeliveryDeadlineDate.ToDateTime(),
            request.GenerateVotingCardsDeadlineDate.ToDateTime(),
            request.DeliveryToPostDeadlineDate.ToDateTime(),
            request.ResetGenerateVotingCardsTriggeredDomainOfInfluenceIds.Select(GuidParser.Parse).ToList());
        return ProtobufEmpty.Instance;
    }

    private ContestSummary CreateContestSummary(Voting.Stimmunterlagen.Data.Models.Contest contest)
    {
        var contestSummary = new ContestSummary(contest);

        if (_appContext.IsPrintJobManagementApp)
        {
            if (contest.ContestDomainOfInfluences != null)
            {
                var printJobStates = contest.ContestDomainOfInfluences.Select(x => x.PrintJob?.State).ToList();

                if (printJobStates.All(state => state == null))
                {
                    contestSummary.PrintJobState = Data.Models.PrintJobState.Empty;
                }
                else
                {
                    contestSummary.PrintJobState = printJobStates
                    .Where(state => state != null)
                    .Min(state => state == Data.Models.PrintJobState.Unspecified
                    ? Data.Models.PrintJobState.Empty
                    : state!.Value);
                }
            }
            else
            {
                contestSummary.PrintJobState = Data.Models.PrintJobState.Empty;
            }
        }

        return contestSummary;
    }
}
