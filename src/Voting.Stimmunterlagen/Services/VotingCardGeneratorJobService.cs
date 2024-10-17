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

public class VotingCardGeneratorJobService : Proto.V1.VotingCardGeneratorJobsService.VotingCardGeneratorJobsServiceBase
{
    private readonly VotingCardGeneratorJobManager _manager;
    private readonly IMapper _mapper;
    private readonly AppContext _appContext;

    public VotingCardGeneratorJobService(VotingCardGeneratorJobManager manager, IMapper mapper, AppContext appContext)
    {
        _manager = manager;
        _mapper = mapper;
        _appContext = appContext;
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<VotingCardGeneratorJobs> ListJobs(ListVotingCardGeneratorJobsRequest request, ServerCallContext context)
    {
        var jobs = await _manager.ListJobs(
            GuidParser.Parse(request.DomainOfInfluenceId),
            _appContext.IsPrintJobManagementApp);

        return _mapper.Map<VotingCardGeneratorJobs>(jobs);
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> RetryJobs(RetryVotingCardGeneratorJobsRequest request, ServerCallContext context)
    {
        await _manager.RetryJobs(GuidParser.Parse(request.DomainOfInfluenceId), context.CancellationToken);
        return ProtobufEmpty.Instance;
    }
}
