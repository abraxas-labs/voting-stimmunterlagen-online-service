// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Stimmregister;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ElectoralRegisterManager
{
    private readonly StimmregisterElectoralRegisterClient _client;
    private readonly IAuth _auth;
    private readonly IDbRepository<VoterListImport> _voterListImportRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly VoterListImportManager _voterListImportManager;
    private readonly EchService _echService;

    public ElectoralRegisterManager(
        StimmregisterElectoralRegisterClient client,
        IAuth auth,
        VoterListImportManager voterListImportManager,
        IDbRepository<VoterListImport> voterListImportRepo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        EchService echService)
    {
        _client = client;
        _auth = auth;
        _voterListImportManager = voterListImportManager;
        _voterListImportRepo = voterListImportRepo;
        _doiRepo = doiRepo;
        _echService = echService;
    }

    public Task<IReadOnlyCollection<ElectoralRegisterFilter>> ListFilters()
        => _client.ListFilters();

    public Task<IReadOnlyCollection<ElectoralRegisterFilterVersion>> ListFilterVersions(Guid filterId)
        => _client.ListFilterVersions(filterId);

    public Task<(ElectoralRegisterFilter Filter, ElectoralRegisterFilterVersion FilterVersion)> GetFilterVersion(Guid filterId)
        => _client.GetFilterVersion(filterId);

    public Task<ElectoralRegisterFilterMetadata> GetFilterMetadata(Guid id, DateOnly deadline)
        => _client.GetFilterMetadata(id, deadline);

    public async Task<(VoterListImport Import, Guid FilterVersionId)> CreateVoterListImportWithNewFilterVersion(Guid domainOfInfluenceId, VoterListImportWithNewElectoralRegisterFilter data, CancellationToken ct)
    {
        await EnsureHasDomainOfInfluenceAccess(domainOfInfluenceId);
        var filterVersionId = await _client.CreateFilterVersion(data.FilterId, data.FilterVersionName, data.FilterVersionDeadline);
        var import = await CreateVoterListImportWithFilterVersionInternal(
            domainOfInfluenceId,
            filterVersionId,
            ct);
        return (import, filterVersionId);
    }

    public async Task<(VoterListImport Import, Guid FilterVersionId)> UpdateVoterListImportWithNewFilterVersion(Guid voterListId, VoterListImportWithNewElectoralRegisterFilter data, CancellationToken ct)
    {
        var existingVoterListImport = await GetVoterListImport(voterListId, ct);
        var filterVersionId = await _client.CreateFilterVersion(data.FilterId, data.FilterVersionName, data.FilterVersionDeadline);
        var import = await UpdateVoterListWithFilterVersionInternal(
            existingVoterListImport,
            filterVersionId,
            ct);
        return (import, filterVersionId);
    }

    private static string BuildName(ElectoralRegisterFilter filter, ElectoralRegisterFilterVersion filterVersion)
        => $"{filter.Name} / {filterVersion.Name}";

    private async Task<VoterListImport> CreateVoterListImportWithFilterVersionInternal(Guid domainOfInfluenceId, Guid filterVersionId, CancellationToken ct)
    {
        var (filter, filterVersion) = await _client.GetFilterVersion(filterVersionId);
        var import = new VoterListImport
        {
            Source = VoterListSource.VotingStimmregisterFilterVersion,
            SourceId = filterVersionId.ToString(),
            Name = BuildName(filter, filterVersion),
            LastUpdate = filterVersion.CreatedAt.Date,
            DomainOfInfluenceId = domainOfInfluenceId,
            AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = true,
        };

        var echData = await _client.StreamEch0045(filterVersionId, ct);
        using var eCh0045Reader = _echService.GetEch0045Reader(echData);
        await _voterListImportManager.Create(import, eCh0045Reader, GetNumberOfVoters(filterVersion), ct);
        return import;
    }

    private async Task<VoterListImport> UpdateVoterListWithFilterVersionInternal(VoterListImport existingVoterListImport, Guid filterVersionId, CancellationToken ct)
    {
        var voterListImport = new VoterListImport
        {
            Id = existingVoterListImport.Id,
            Source = VoterListSource.VotingStimmregisterFilterVersion,
            SourceId = filterVersionId.ToString(),
            Name = existingVoterListImport.Name,
            LastUpdate = existingVoterListImport.LastUpdate,
            DomainOfInfluenceId = existingVoterListImport.DomainOfInfluenceId,
            AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = true,
        };

        if (string.Equals(existingVoterListImport.SourceId, voterListImport.SourceId, StringComparison.Ordinal))
        {
            return voterListImport;
        }

        var (filter, filterVersion) = await _client.GetFilterVersion(filterVersionId);
        voterListImport.Name = BuildName(filter, filterVersion);
        voterListImport.LastUpdate = filterVersion.CreatedAt.Date;
        var echData = await _client.StreamEch0045(filterVersionId, ct);
        using var eCh0045Reader = _echService.GetEch0045Reader(echData);
        await _voterListImportManager.Update(voterListImport, eCh0045Reader, GetNumberOfVoters(filterVersion), ct);
        return voterListImport;
    }

    private int GetNumberOfVoters(ElectoralRegisterFilterVersion filterVersion) =>
        filterVersion.NumberOfPersons - filterVersion.NumberOfInvalidPersons;

    private async Task EnsureHasDomainOfInfluenceAccess(Guid doiId)
    {
        var hasAccess = await _doiRepo.Query()
            .WhereIsManager(_auth.Tenant.Id)
            .AnyAsync(x => x.Id == doiId);

        if (!hasAccess)
        {
            throw new ForbiddenException("No access to this domain of influence");
        }
    }

    private async Task<VoterListImport> GetVoterListImport(Guid importId, CancellationToken ct)
    {
        return await _voterListImportRepo
              .Query()
              .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
              .Where(x => x.Id == importId && x.Source == VoterListSource.VotingStimmregisterFilterVersion)
              .FirstOrDefaultAsync(ct)
              ?? throw new EntityNotFoundException(nameof(VoterListImport), importId);
    }
}
