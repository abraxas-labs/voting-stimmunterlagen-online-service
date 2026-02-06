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
using Ech0045Version = Voting.Stimmunterlagen.Data.Models.Ech0045Version;

namespace Voting.Stimmunterlagen.Services;

public class ContestEVotingExportJobService : Proto.V1.ContestEVotingExportJobService.ContestEVotingExportJobServiceBase
{
    private readonly ContestEVotingExportJobManager _manager;
    private readonly IMapper _mapper;

    public ContestEVotingExportJobService(ContestEVotingExportJobManager manager, IMapper mapper)
    {
        _manager = manager;
        _mapper = mapper;
    }

    [AuthorizeElectionAdmin]
    public override async Task<ContestEVotingExportJob> GetJob(GetContestEVotingExportJobRequest request, ServerCallContext context)
    {
        var job = await _manager.GetJob(GuidParser.Parse(request.ContestId));
        return _mapper.Map<ContestEVotingExportJob>(job);
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> RetryJob(RetryContestEVotingExportJobRequest request, ServerCallContext context)
    {
        await _manager.RetryJob(GuidParser.Parse(request.ContestId));
        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> UpdateAndResetJob(UpdateAndResetContestEVotingExportJobRequest request, ServerCallContext context)
    {
        await _manager.UpdateAndResetJob(GuidParser.Parse(request.ContestId), _mapper.Map<Ech0045Version>(request.Ech0045Version));
        return ProtobufEmpty.Instance;
    }
}
