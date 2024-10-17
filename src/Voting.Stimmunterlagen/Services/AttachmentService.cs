// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Lib.Grpc;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voting.Stimmunterlagen.Proto.V1.Responses;

namespace Voting.Stimmunterlagen.Services;

public class AttachmentService : Proto.V1.AttachmentService.AttachmentServiceBase
{
    private readonly IMapper _mapper;
    private readonly AttachmentManager _attachmentManager;
    private readonly AppContext _appContext;

    public AttachmentService(IMapper mapper, AttachmentManager attachmentManager, AppContext appContext)
    {
        _mapper = mapper;
        _attachmentManager = attachmentManager;
        _appContext = appContext;
    }

    [AuthorizeElectionAdminOrPrintJobManager]
    public override async Task<AttachmentCategorySummaries> ListCategorySummaries(ListAttachmentCategorySummariesRequest request, ServerCallContext context)
    {
        switch (request.DoiOrFilterCase)
        {
            case ListAttachmentCategorySummariesRequest.DoiOrFilterOneofCase.DomainOfInfluenceId:
                return _mapper.Map<AttachmentCategorySummaries>(await _attachmentManager.ListCategorySummariesForDomainOfInfluence(
                    GuidParser.Parse(request.DomainOfInfluenceId),
                    _appContext.IsVotingDocumentsApp));
            case ListAttachmentCategorySummariesRequest.DoiOrFilterOneofCase.Filter:
                return _mapper.Map<AttachmentCategorySummaries>(await _attachmentManager.ListCategorySummariesForFilter(
                    GuidParser.Parse(request.Filter!.ContestId),
                    request.Filter.Query,
                    request.Filter.State == AttachmentState.Unspecified ? null : _mapper.Map<Data.Models.AttachmentState>(request.Filter.State),
                    _appContext.IsPrintJobManagementApp));
            default:
                throw new ValidationException(
                $"either {nameof(ListAttachmentCategorySummariesRequest.DoiOrFilterOneofCase.DomainOfInfluenceId)} or {nameof(ListAttachmentCategorySummariesRequest.DoiOrFilterOneofCase.Filter)} must be set");
        }
    }

    [AuthorizeElectionAdmin]
    public override async Task<IdValue> Create(CreateAttachmentRequest request, ServerCallContext context)
    {
        var id = await _attachmentManager.Create(
            _mapper.Map<Data.Models.Attachment>(request), request.RequiredCount);
        return new IdValue { Id = id.ToString() };
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> Update(UpdateAttachmentRequest request, ServerCallContext context)
    {
        await _attachmentManager.Update(_mapper.Map<Data.Models.Attachment>(request), request.RequiredCount);
        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> Delete(IdValueRequest request, ServerCallContext context)
    {
        await _attachmentManager.Delete(request.GetId());
        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> AssignPoliticalBusiness(
        AssignPoliticalBusinessAttachmentRequest request,
        ServerCallContext context)
    {
        await _attachmentManager.AssignPoliticalBusiness(
            GuidParser.Parse(request.Id),
            GuidParser.Parse(request.PoliticalBusinessId));

        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> UnassignPoliticalBusiness(
        UnassignPoliticalBusinessAttachmentRequest request,
        ServerCallContext context)
    {
        await _attachmentManager.UnassignPoliticalBusiness(
            GuidParser.Parse(request.Id),
            GuidParser.Parse(request.PoliticalBusinessId));

        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> SetDomainOfInfluenceAttachmentRequiredCount(
        SetDomainOfInfluenceAttachmentRequiredCountRequest request,
        ServerCallContext context)
    {
        await _attachmentManager.SetDomainOfInfluenceAttachmentRequiredCount(
            GuidParser.Parse(request.Id),
            GuidParser.Parse(request.DomainOfInfluenceId),
            request.RequiredCount);

        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> SetStation(
        SetAttachmentStationRequest request,
        ServerCallContext context)
    {
        await _attachmentManager.SetStation(
            GuidParser.Parse(request.Id),
            request.Station);

        return ProtobufEmpty.Instance;
    }

    [AuthorizePrintJobManager]
    public override async Task<Empty> SetState(
        SetAttachmentStateRequest request,
        ServerCallContext context)
    {
        await _attachmentManager.SetState(
            GuidParser.Parse(request.Id),
            _mapper.Map<Data.Models.AttachmentState>(request.State),
            request.Comment);

        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<DomainOfInfluenceAttachmentCategorySummariesEntries> ListDomainOfInfluenceAttachmentCategorySummaries(
        ListDomainOfInfluenceAttachmentCategorySummariesRequest request,
        ServerCallContext context)
    {
        var entries = await _attachmentManager.ListDomainOfInfluenceAttachmentCategorySummaries(GuidParser.Parse(request.DomainOfInfluenceId));
        return _mapper.Map<DomainOfInfluenceAttachmentCategorySummariesEntries>(entries);
    }

    [AuthorizeElectionAdmin]
    public override async Task<Empty> UpdateDomainOfInfluenceAttachmentEntries(UpdateDomainOfInfluenceAttachmentEntriesRequest request, ServerCallContext context)
    {
        await _attachmentManager.UpdateDomainOfInfluenceEntries(
            GuidParser.Parse(request.Id),
            request.DomainOfInfluenceIds.Select(GuidParser.Parse).ToList());

        return ProtobufEmpty.Instance;
    }

    [AuthorizeElectionAdmin]
    public override async Task<DomainOfInfluenceAttachmentCounts> ListDomainOfInfluenceAttachmentCounts(ListDomainOfInfluenceAttachmentCountsRequest request, ServerCallContext context)
    {
        var entries = await _attachmentManager.ListDomainOfInfluenceAttachmentCounts(GuidParser.Parse(request.AttachmentId));
        return _mapper.Map<DomainOfInfluenceAttachmentCounts>(entries);
    }

    [AuthorizeElectionAdmin]
    public override async Task<GetAttachmentsProgressResponse> GetAttachmentsProgress(GetAttachmentsProgressRequest request, ServerCallContext context)
    {
        var stationsSet = await _attachmentManager.HasAttachmentsStationSet(GuidParser.Parse(request.ContestId));
        return new() { StationsSet = stationsSet };
    }
}
