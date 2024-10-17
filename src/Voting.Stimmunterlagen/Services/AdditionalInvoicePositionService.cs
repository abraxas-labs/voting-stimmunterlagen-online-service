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
using Voting.Stimmunterlagen.Proto.V1.Responses;
using AdditionalInvoicePosition = Voting.Stimmunterlagen.Data.Models.AdditionalInvoicePosition;

namespace Voting.Stimmunterlagen.Services;

[AuthorizePrintJobManager]
public class AdditionalInvoicePositionService : Proto.V1.AdditionalInvoicePositionService.AdditionalInvoicePositionServiceBase
{
    private readonly AppContext _appContext;
    private readonly AdditionalInvoicePositionManager _positionManager;
    private readonly IMapper _mapper;

    public AdditionalInvoicePositionService(AppContext appContext, AdditionalInvoicePositionManager positionManager, IMapper mapper)
    {
        _appContext = appContext;
        _positionManager = positionManager;
        _mapper = mapper;
    }

    public async override Task<AdditionalInvoicePositions> List(ListAdditionalInvoicePositionsRequest request, ServerCallContext context)
    {
        var positions = await _positionManager.ListAdditionalInvoicePositions(GuidParser.Parse(request.ContestId), _appContext.IsPrintJobManagementApp);
        return _mapper.Map<AdditionalInvoicePositions>(positions);
    }

    public override async Task<IdValue> Create(CreateAdditionalInvoicePositionRequest request, ServerCallContext context)
    {
        var id = await _positionManager.CreateAdditionalInvoicePosition(_mapper.Map<AdditionalInvoicePosition>(request));
        return new IdValue { Id = id.ToString() };
    }

    public override async Task<Empty> Update(UpdateAdditionalInvoicePositionRequest request, ServerCallContext context)
    {
        await _positionManager.UpdateAdditionalInvoicePosition(_mapper.Map<AdditionalInvoicePosition>(request));
        return ProtobufEmpty.Instance;
    }

    public override async Task<Empty> Delete(IdValueRequest request, ServerCallContext context)
    {
        await _positionManager.DeleteAdditionalInvoicePosition(request.GetId());
        return ProtobufEmpty.Instance;
    }

    public override Task<ListAdditionalInvoicePositionAvailableMaterialResponse> ListAvailableMaterial(Empty request, ServerCallContext context)
    {
        return Task.FromResult(_mapper.Map<ListAdditionalInvoicePositionAvailableMaterialResponse>(
            _positionManager.GetAvailableMaterials()));
    }
}
