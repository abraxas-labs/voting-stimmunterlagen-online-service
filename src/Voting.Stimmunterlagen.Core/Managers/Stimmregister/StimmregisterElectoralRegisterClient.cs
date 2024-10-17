// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Voting.Lib.Common;
using Voting.Stimmregister.Proto.V1.Services;
using Voting.Stimmregister.Proto.V1.Services.Requests;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Core.Managers.Stimmregister;

public class StimmregisterElectoralRegisterClient
{
    private static readonly Uri EchExportApiPath = new("v1/export/ech-0045", UriKind.Relative);
    private readonly FilterService.FilterServiceClient _filterClient;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;

    public StimmregisterElectoralRegisterClient(FilterService.FilterServiceClient filterClient, IMapper mapper, HttpClient httpClient)
    {
        _filterClient = filterClient;
        _mapper = mapper;
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<ElectoralRegisterFilter>> ListFilters()
    {
        var filters = await _filterClient.GetAllAsync(new FilterServiceGetAllRequest());
        return _mapper.Map<IReadOnlyCollection<ElectoralRegisterFilter>>(filters.Filters);
    }

    public async Task<IReadOnlyCollection<ElectoralRegisterFilterVersion>> ListFilterVersions(Guid filterId)
    {
        var filter = await _filterClient.GetSingleAsync(new FilterServiceGetSingleRequest { FilterId = filterId.ToString() });
        return _mapper.Map<IReadOnlyCollection<ElectoralRegisterFilterVersion>>(filter.Filter.Versions);
    }

    public async Task<(ElectoralRegisterFilter Filter, ElectoralRegisterFilterVersion FilterVersion)> GetFilterVersion(Guid filterVersionId)
    {
        var filterVersion = await _filterClient.GetSingleVersionAsync(new FilterServiceGetSingleVersionRequest { FilterVersionId = filterVersionId.ToString() });
        return (
            _mapper.Map<ElectoralRegisterFilter>(filterVersion.Filter),
            _mapper.Map<ElectoralRegisterFilterVersion>(filterVersion.FilterVersion));
    }

    public async Task<ElectoralRegisterFilterMetadata> GetFilterMetadata(Guid id, DateOnly deadline)
    {
        var metadata = await _filterClient.GetMetadataAsync(new FilterServicePreviewMetadataRequest
        {
            FilterId = id.ToString(),
            Deadline = deadline.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToTimestamp(),
        });
        return new ElectoralRegisterFilterMetadata(
            metadata.CountOfPersons,
            metadata.CountOfInvalidPersons,
            metadata.IsActual,
            metadata.ActualityDate?.ToDateTime());
    }

    public async Task<Guid> CreateFilterVersion(Guid filterId, string name, DateOnly deadLine)
    {
        var response = await _filterClient.CreateVersionAsync(new FilterServiceCreateFilterVersionRequest
        {
            FilterId = filterId.ToString(),
            Name = name,
            Deadline = DateTime.SpecifyKind(deadLine.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).ToTimestamp(),
        });
        return GuidParser.Parse(response.Id);
    }

    public async Task<Stream> StreamEch0045(Guid filterVersionId, CancellationToken ct)
    {
        var request = new EchExportRequest(filterVersionId);
        var response = await _httpClient.PostAsJsonAsync(EchExportApiPath, request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(ct);
    }

    private record EchExportRequest(Guid VersionId);
}
