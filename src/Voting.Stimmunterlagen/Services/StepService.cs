// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers.Steps;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Step = Voting.Stimmunterlagen.Data.Models.Step;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class StepService : Proto.V1.StepService.StepServiceBase
{
    private readonly IMapper _mapper;
    private readonly StepManager _stepManager;

    public StepService(IMapper mapper, StepManager stepManager)
    {
        _mapper = mapper;
        _stepManager = stepManager;
    }

    public override async Task<StepStates> List(ListStepsRequest request, ServerCallContext context)
    {
        var steps = await _stepManager.List(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<StepStates>(steps);
    }

    public override async Task<Empty> Approve(ApproveStepRequest request, ServerCallContext context)
    {
        await _stepManager.Approve(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<Step>(request.Step),
            context.CancellationToken);
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Revert(RevertStepRequest request, ServerCallContext context)
    {
        await _stepManager.Revert(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _mapper.Map<Step>(request.Step),
            context.CancellationToken);
        return ProtobufEmpty.Instance;
    }
}
