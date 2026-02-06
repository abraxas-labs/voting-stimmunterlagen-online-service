// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Stimmregister;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
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
    private readonly Ech0045Service _ech0045Service;
    private readonly IClock _clock;

    public ElectoralRegisterManager(
        StimmregisterElectoralRegisterClient client,
        IAuth auth,
        VoterListImportManager voterListImportManager,
        IDbRepository<VoterListImport> voterListImportRepo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        Ech0045Service ech0045Service,
        IClock clock)
    {
        _client = client;
        _auth = auth;
        _voterListImportManager = voterListImportManager;
        _voterListImportRepo = voterListImportRepo;
        _doiRepo = doiRepo;
        _ech0045Service = ech0045Service;
        _clock = clock;
    }

    public Task<IReadOnlyCollection<ElectoralRegisterFilter>> ListFilters()
        => _client.ListFilters();

    public Task<IReadOnlyCollection<ElectoralRegisterFilterVersion>> ListFilterVersions(Guid filterId)
        => _client.ListFilterVersions(filterId);

    public Task<(ElectoralRegisterFilter Filter, ElectoralRegisterFilterVersion FilterVersion)> GetFilterVersion(Guid filterId)
        => _client.GetFilterVersion(filterId);

    public Task<ElectoralRegisterFilterMetadata> GetFilterMetadata(Guid id, DateOnly deadline)
        => _client.GetFilterMetadata(id, deadline);

    public async Task<(VoterListImportResult Result, Guid FilterVersionId)> CreateVoterListImportWithNewFilterVersion(Guid domainOfInfluenceId, VoterListImportWithNewElectoralRegisterFilter data, CancellationToken ct)
    {
        await EnsureCanCreateOrUpdate(domainOfInfluenceId);
        var filterVersionId = await _client.CreateFilterVersion(data.FilterId, data.FilterVersionName, data.FilterVersionDeadline);
        var result = await CreateVoterListImportWithFilterVersionInternal(
            domainOfInfluenceId,
            filterVersionId,
            ct);
        return (result, filterVersionId);
    }

    public async Task<(VoterListImportResult Result, Guid FilterVersionId)> UpdateVoterListImportWithNewFilterVersion(Guid voterListId, VoterListImportWithNewElectoralRegisterFilter data, CancellationToken ct)
    {
        var existingVoterListImport = await GetVoterListImport(voterListId, ct);
        await EnsureCanCreateOrUpdate(existingVoterListImport.DomainOfInfluenceId);
        var filterVersionId = await _client.CreateFilterVersion(data.FilterId, data.FilterVersionName, data.FilterVersionDeadline);
        var result = await UpdateVoterListWithFilterVersionInternal(
            existingVoterListImport,
            filterVersionId,
            ct);
        return (result, filterVersionId);
    }

    private static string BuildName(ElectoralRegisterFilter filter, ElectoralRegisterFilterVersion filterVersion)
        => $"{filter.Name} / {filterVersion.Name}";

    private async Task<VoterListImportResult> CreateVoterListImportWithFilterVersionInternal(Guid domainOfInfluenceId, Guid filterVersionId, CancellationToken ct)
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
        using var eCh0045Reader = _ech0045Service.GetEch0045Reader(Ech0045Version.V4, echData);
        return await _voterListImportManager.Create(import, eCh0045Reader, GetNumberOfVoters(filterVersion), ct);
    }

    private async Task<VoterListImportResult> UpdateVoterListWithFilterVersionInternal(VoterListImport existingVoterListImport, Guid filterVersionId, CancellationToken ct)
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
            return new VoterListImportResult
            {
                Import = voterListImport,
            };
        }

        var (filter, filterVersion) = await _client.GetFilterVersion(filterVersionId);
        voterListImport.Name = BuildName(filter, filterVersion);
        voterListImport.LastUpdate = filterVersion.CreatedAt.Date;
        var echData = await _client.StreamEch0045(filterVersionId, ct);
        using var eCh0045Reader = _ech0045Service.GetEch0045Reader(Ech0045Version.V4, echData);
        return await _voterListImportManager.Update(voterListImport, eCh0045Reader, GetNumberOfVoters(filterVersion), ct);
    }

    private int GetNumberOfVoters(ElectoralRegisterFilterVersion filterVersion) =>
        filterVersion.NumberOfPersons - filterVersion.NumberOfInvalidPersons;

    private async Task EnsureCanCreateOrUpdate(Guid doiId)
    {
        var doi = await _doiRepo.Query()
            .Include(x => x.Contest!)
            .Include(x => x.CountingCircles!)
                .ThenInclude(x => x.CountingCircle)
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new ForbiddenException("No access to this domain of influence");

        if (!doi.CountingCircles!.Any(doiCc => doiCc.CountingCircle!.EVoting) || !doi.Contest!.EVoting)
        {
            return;
        }

        if (doi.Contest!.ElectoralRegisterEVotingFrom.HasValue && doi.Contest.ElectoralRegisterEVotingFrom > _clock.UtcNow)
        {
            throw new ForbiddenException("Cannot create or update electoral registers yet because electoral register e-voting is not active yet");
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
