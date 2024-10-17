// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voting.Stimmunterlagen.Proto.V1.Responses;
using ElectoralRegisterFilter = Voting.Stimmunterlagen.Proto.V1.Models.ElectoralRegisterFilter;
using ElectoralRegisterFilterMetadata = Voting.Stimmunterlagen.Proto.V1.Models.ElectoralRegisterFilterMetadata;
using ElectoralRegisterFilterVersion = Voting.Stimmunterlagen.Proto.V1.Models.ElectoralRegisterFilterVersion;

namespace Voting.Stimmunterlagen.Services;

[AuthorizeElectionAdmin]
public class ElectoralRegisterService : Proto.V1.ElectoralRegisterService.ElectoralRegisterServiceBase
{
    private readonly IMapper _mapper;
    private readonly ElectoralRegisterManager _manager;

    public ElectoralRegisterService(IMapper mapper, ElectoralRegisterManager manager)
    {
        _mapper = mapper;
        _manager = manager;
    }

    public override async Task<ElectoralRegisterFilters> ListFilters(Empty request, ServerCallContext context)
    {
        var filters = await _manager.ListFilters();
        return _mapper.Map<ElectoralRegisterFilters>(filters);
    }

    public override async Task<ElectoralRegisterFilterVersions> ListFilterVersions(ListElectoralRegisterFilterVersionsRequest request, ServerCallContext context)
    {
        var versions = await _manager.ListFilterVersions(GuidParser.Parse(request.FilterId));
        return _mapper.Map<ElectoralRegisterFilterVersions>(versions);
    }

    public override async Task<ElectoralRegisterFilterMetadata> GetFilterMetadata(GetElectoralRegisterFilterMetadataRequest request, ServerCallContext context)
    {
        var id = GuidParser.Parse(request.FilterId);
        var deadline = DateOnly.FromDateTime(request.Deadline.ToDateTime());
        var metadata = await _manager.GetFilterMetadata(id, deadline);
        return _mapper.Map<ElectoralRegisterFilterMetadata>(metadata);
    }

    public override async Task<GetElectoralRegisterFilterVersionResponse> GetFilterVersion(GetElectoralRegisterFilterVersionRequest request, ServerCallContext context)
    {
        var (filter, filterVersion) = await _manager.GetFilterVersion(GuidParser.Parse(request.FilterVersionId));
        return new GetElectoralRegisterFilterVersionResponse
        {
            Filter = _mapper.Map<ElectoralRegisterFilter>(filter),
            Version = _mapper.Map<ElectoralRegisterFilterVersion>(filterVersion),
        };
    }

    public override async Task<VoterListImportWithElectoralRegisterResponse> CreateVoterListImportWithNewFilterVersion(CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest request, ServerCallContext context)
    {
        var data = _mapper.Map<VoterListImportWithNewElectoralRegisterFilter>(request);
        var (import, filterVersionId) = await _manager.CreateVoterListImportWithNewFilterVersion(
            GuidParser.Parse(request.DomainOfInfluenceId),
            data,
            context.CancellationToken);

        return new VoterListImportWithElectoralRegisterResponse
        {
            ImportId = import.Id.ToString(),
            FilterVersionId = filterVersionId.ToString(),
            VoterLists = { _mapper.Map<List<VoterListImportVoterListResponse>>(import.VoterLists) },
        };
    }

    public override async Task<VoterListImportWithElectoralRegisterResponse> UpdateVoterListImportWithNewFilterVersion(UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest request, ServerCallContext context)
    {
        var data = _mapper.Map<VoterListImportWithNewElectoralRegisterFilter>(request);
        var (import, filterVersionId) = await _manager.UpdateVoterListImportWithNewFilterVersion(
            GuidParser.Parse(request.ImportId),
            data,
            context.CancellationToken);

        return new VoterListImportWithElectoralRegisterResponse
        {
            ImportId = request.ImportId,
            FilterVersionId = filterVersionId.ToString(),
            VoterLists = { _mapper.Map<List<VoterListImportVoterListResponse>>(import.VoterLists) },
        };
    }
}
