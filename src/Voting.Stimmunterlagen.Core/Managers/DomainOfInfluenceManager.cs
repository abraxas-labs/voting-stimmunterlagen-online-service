// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class DomainOfInfluenceManager
{
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<ContestDomainOfInfluenceHierarchyEntry> _doiHierarchyEntryRepo;
    private readonly IAuth _auth;
    private readonly IClock _clock;

    public DomainOfInfluenceManager(
        IDbRepository<Contest> contestRepo,
        IAuth auth,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<ContestDomainOfInfluenceHierarchyEntry> doiHierarchyEntryRepo,
        IClock clock)
    {
        _contestRepo = contestRepo;
        _auth = auth;
        _doiRepo = doiRepo;
        _doiHierarchyEntryRepo = doiHierarchyEntryRepo;
        _clock = clock;
    }

    public async Task<List<ContestDomainOfInfluence>> ListManagedByCurrentTenant(Guid contestId)
    {
        var tenantId = _auth.Tenant.Id;
        return await _contestRepo.Query()
            .Where(x => x.Id == contestId)
            .SelectMany(x => x.ContestDomainOfInfluences!)
            .Where(x => x.SecureConnectId == tenantId && (x.Role == ContestRole.Manager || (x.Role == ContestRole.Attendee && x.Contest!.Approved.HasValue)))
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<ContestDomainOfInfluence>> ListChildren(Guid domainOfInfluenceId)
    {
        var childIds = await _doiHierarchyEntryRepo
            .Query()
            .Where(x => x.ParentDomainOfInfluenceId == domainOfInfluenceId)
            .Select(x => x.DomainOfInfluenceId)
            .ToListAsync();

        return await _doiRepo.Query()
            .Include(x => x.Contest)
            .WhereCanRead(_auth.Tenant.Id)
            .Where(x => childIds.Contains(x.Id))
            .WhereUsesVotingCardsInCurrentContest()
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<ContestDomainOfInfluence> Get(Guid id)
    {
        var tenantId = _auth.Tenant.Id;
        return await _doiRepo.Query()
                   .WhereCanRead(tenantId)
                   .Include(x => x.CountingCircles!)
                   .ThenInclude(x => x.CountingCircle)
                   .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), id);
    }

    public async Task<List<EVotingDomainOfInfluenceEntry>> ListEVoting(Guid contestId)
    {
        var tenantId = _auth.Tenant.Id;
        var dois = await _doiRepo.Query()
            .Where(d => d.ContestId == contestId && (d.Role == ContestRole.Manager || d.Role == ContestRole.Attendee) && d.ResponsibleForVotingCards)
            .WhereIsContestManager(tenantId)
            .Include(d => d.CountingCircles!).ThenInclude(doiCc => doiCc.CountingCircle)
            .Include(d => d.PoliticalBusinessPermissionEntries)
            .Include(d => d.VoterLists)
            .Include(d => d.StepStates)
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return dois.ConvertAll(doi => new EVotingDomainOfInfluenceEntry(doi));
    }

    public async Task UpdateSettings(Guid doiId, bool allowManualVoterListUpload)
    {
        var doi = await _doiRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        doi.AllowManualVoterListUpload = allowManualVoterListUpload;
        await _doiRepo.Update(doi);
    }

    public async Task UpdateLastVoterUpdate(Guid doiId)
    {
        var doi = await _doiRepo.Query()
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        doi.LastVoterUpdate = _clock.UtcNow;
        await _doiRepo.Update(doi);
    }

    internal async Task<string> GetSecureConnectId(Guid id)
    {
        return await _doiRepo
            .Query()
            .Where(x => x.Id == id)
            .Select(x => x.SecureConnectId)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), id);
    }

    internal async Task<List<ContestDomainOfInfluence>> GetParentsAndSelf(Guid id)
    {
        var doi = await _doiRepo.Query()
            .Include(x => x.HierarchyEntries!)
            .ThenInclude(x => x.ParentDomainOfInfluence)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), id);

        return (doi.HierarchyEntries?.Select(x => x.ParentDomainOfInfluence) ?? new List<ContestDomainOfInfluence>())
            .Append(doi)
            .WhereNotNull()
            .ToList();
    }

    internal async Task<Dictionary<Guid, List<ContestDomainOfInfluence>>> GetParentsAndSelfPerDoi(List<Guid> ids)
    {
        var dois = await _doiRepo.Query()
            .Include(x => x.HierarchyEntries!)
            .ThenInclude(x => x.ParentDomainOfInfluence)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        var parentsAndSelfByDoiId = new Dictionary<Guid, List<ContestDomainOfInfluence>>();

        foreach (var doi in dois)
        {
            var parentsAndSelf = (doi.HierarchyEntries?.Select(x => x.ParentDomainOfInfluence) ?? new List<ContestDomainOfInfluence>())
                .Append(doi)
                .WhereNotNull()
                .ToList();
            parentsAndSelfByDoiId[doi.Id] = parentsAndSelf;
        }

        return parentsAndSelfByDoiId;
    }
}
