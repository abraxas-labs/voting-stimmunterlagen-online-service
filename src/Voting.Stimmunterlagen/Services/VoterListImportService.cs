// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class VoterListImportService : Proto.V1.VoterListImportService.VoterListImportServiceBase
{
    private readonly IMapper _mapper;
    private readonly VoterListImportManager _manager;

    public VoterListImportService(IMapper mapper, VoterListImportManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<VoterListImport> Get(IdValueRequest request, ServerCallContext context)
    {
        var import = await _manager.Get(request.GetId());
        return _mapper.Map<VoterListImport>(import);
    }

    public override async Task<Empty> Delete(IdValueRequest request, ServerCallContext context)
    {
        await _manager.Delete(request.GetId());
        return ProtobufEmpty.Instance;
    }
}
