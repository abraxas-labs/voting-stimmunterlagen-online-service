// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    private readonly IDbRepository<StepState> _stepStateRepo;

    public DomainOfInfluenceManager(
        IDbRepository<Contest> contestRepo,
        IAuth auth,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<ContestDomainOfInfluenceHierarchyEntry> doiHierarchyEntryRepo,
        IClock clock,
        IDbRepository<StepState> stepStateRepo)
    {
        _contestRepo = contestRepo;
        _auth = auth;
        _doiRepo = doiRepo;
        _doiHierarchyEntryRepo = doiHierarchyEntryRepo;
        _clock = clock;
        _stepStateRepo = stepStateRepo;
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
            .WhereIsManagerOrParentManager(_auth.Tenant.Id)
            .Where(x => childIds.Contains(x.Id))
            .WhereUsesVotingCardsInCurrentContest()
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<ContestDomainOfInfluence>> ListPoliticalBusinessAttendees(Guid domainOfInfluenceId)
    {
        var doi = await _doiRepo.Query()
            .Include(doi => doi.CountingCircles!)
                .ThenInclude(doiCc => doiCc.CountingCircle)
            .Include(doi => doi.ParentHierarchyEntries!)
                .ThenInclude(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries)
            .Include(doi => doi.ParentHierarchyEntries!)
                .ThenInclude(x => x.DomainOfInfluence!.Contest)
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == domainOfInfluenceId);

        if (doi == null)
        {
            return new();
        }

        var mainVotingCardsDois = await GetMainVotingCardsDomainOfInfluencesByContestId(doi.ContestId);
        var attendees = ListPoliticalBusinessAttendees(doi, mainVotingCardsDois.GetValueOrDefault(doi.ContestId) ?? new());

        return attendees
            .DistinctBy(doi => doi!.Id)
            .OrderBy(doi => doi.Name)
            .ToList()!;
    }

    public async Task<ContestDomainOfInfluence> Get(Guid id)
    {
        var tenantId = _auth.Tenant.Id;
        return await _doiRepo.Query()
                   .WhereIsManagerOrParentManager(tenantId)
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
            .WhereUsesVotingCardsInCurrentContest()
            .WhereIsContestManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        doi.AllowManualVoterListUpload = allowManualVoterListUpload;
        await _doiRepo.Update(doi);
    }

    public async Task SetCountOfEmptyVotingCards(Guid doiId, int countOfEmptyVotingCards)
    {
        await EnsureAttachmentStepIsApproved(doiId);
        var doi = await _doiRepo.Query()
            .Where(doi => doi.HasEmptyVotingCards)
            .WhereUsesVotingCardsInCurrentContest()
            .WhereContestNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .WhereIsManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);

        doi.CountOfEmptyVotingCards = countOfEmptyVotingCards;
        doi.LastCountOfEmptyVotingCardsUpdate = _clock.UtcNow;
        await _doiRepo.Update(doi);
    }

    public async Task UpdateLatestVoterListImportsLastUpdate(Guid doiId)
    {
        var affectedRows = await _doiRepo.Query()
            .WhereIsManager(_auth.Tenant.Id)
            .Where(x => x.Id == doiId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                doi => doi.LatestVoterListImportsLastUpdate,
                doi => _doiRepo.Query()
                    .Where(x => x.Id == doi.Id)
                    .SelectMany(x => x.VoterListImports!)
                    .Max(i => (DateTime?)i.LastUpdate)));

        if (affectedRows == 0)
        {
            throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        }
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

    internal List<ContestDomainOfInfluence> ListPoliticalBusinessAttendees(ContestDomainOfInfluence domainOfInfluence, List<ContestDomainOfInfluence> mainVotingCardsDomainOfInfluences)
    {
        var doiAttendees = ListAttendees(domainOfInfluence, mainVotingCardsDomainOfInfluences);

        if (doiAttendees.Any(doi => doi.PoliticalBusinessPermissionEntries == null || doi.Contest == null))
        {
            throw new InvalidOperationException("To load political business attendees, the fields permission entries and contest must be included");
        }

        return doiAttendees
            .Where(doi => doi.UsesVotingCardsInCurrentContest())
            .ToList();
    }

    /// <summary>
    /// Returns a list of attendees of a domain of influence. This includes child domain of influences
    /// and related main domain of influences (if they are responsible for voting cards).
    /// </summary>
    /// <param name="domainOfInfluence">The domain of influence.</param>
    /// <param name="mainVotingCardsDomainOfInfluences">The main voting card domain of influences of the related contest.</param>
    /// <returns>A list of attendees.</returns>
    internal List<ContestDomainOfInfluence> ListAttendees(ContestDomainOfInfluence domainOfInfluence, List<ContestDomainOfInfluence> mainVotingCardsDomainOfInfluences)
    {
        var hierarchyAttendees = domainOfInfluence.ParentHierarchyEntries!
            .Where(x => x.DomainOfInfluence!.ResponsibleForVotingCards)
            .Select(x => x.DomainOfInfluence)
            .Distinct()
            .ToList();

        var mainVotingCardsAttendees = mainVotingCardsDomainOfInfluences
            .Where(x => IsMainVotingCardsAttendee(x, domainOfInfluence))
            .ToList();

        return hierarchyAttendees
            .Concat(mainVotingCardsAttendees)
            .Where(doi => doi!.Id != domainOfInfluence.Id)
            .DistinctBy(doi => doi!.Id)
            .ToList()!;
    }

    internal Dictionary<Guid, List<ContestDomainOfInfluence>> BuildHostsByAttendeeId(
        IReadOnlyCollection<ContestDomainOfInfluence> attendees,
        IReadOnlyCollection<ContestDomainOfInfluence> contestDomainOfInfluences,
        IReadOnlyCollection<ContestDomainOfInfluence> mainVotingCardsDomainOfInfluences)
    {
        var mainVotingCardsDoisBySecureConnectId = mainVotingCardsDomainOfInfluences
            .ToLookup(x => x.SecureConnectId);

        var hostsByAttendeeId = new Dictionary<Guid, List<ContestDomainOfInfluence>>();

        foreach (var hostCandidate in contestDomainOfInfluences)
        {
            var hostCandidateSecureConnectIds = hostCandidate.CountingCircles!
                .Select(doiCc => doiCc.CountingCircle!.SecureConnectId)
                .Prepend(hostCandidate.SecureConnectId)
                .Distinct();

            foreach (var hostCandidateSecureConnectId in hostCandidateSecureConnectIds)
            {
                foreach (var mainVotingCardDoi in mainVotingCardsDoisBySecureConnectId[hostCandidateSecureConnectId])
                {
                    if (!IsMainVotingCardsAttendee(mainVotingCardDoi, hostCandidate))
                    {
                        continue;
                    }

                    if (!hostsByAttendeeId.TryGetValue(mainVotingCardDoi.Id, out var hosts))
                    {
                        hostsByAttendeeId[mainVotingCardDoi.Id] = hosts = new();
                    }

                    hosts.Add(hostCandidate);
                }
            }
        }

        foreach (var attendee in attendees)
        {
            var parentHosts = attendee.HierarchyEntries!.Select(x => x.ParentDomainOfInfluence!);
            var mainVotingCardsHosts = hostsByAttendeeId.GetValueOrDefault(attendee.Id) ?? new();
            hostsByAttendeeId[attendee.Id] = parentHosts.Concat(mainVotingCardsHosts).DistinctBy(x => x.Id).ToList();
        }

        return hostsByAttendeeId;
    }

    internal Task<Dictionary<Guid, List<ContestDomainOfInfluence>>> GetMainVotingCardsDomainOfInfluencesByContestId(Guid contestId)
        => GetMainVotingCardsDomainOfInfluencesByContestId(new[] { contestId });

    internal async Task<Dictionary<Guid, List<ContestDomainOfInfluence>>> GetMainVotingCardsDomainOfInfluencesByContestId(IReadOnlyCollection<Guid>? contestIds = null)
    {
        return await _doiRepo.Query()
            .Include(doi => doi.PoliticalBusinessPermissionEntries)
            .Include(doi => doi.Contest)
            .WhereContestInTestingPhase()
            .Where(doi => doi.IsMainVotingCardsDomainOfInfluence
                && (contestIds == null || contestIds.Contains(doi.ContestId)))
            .GroupBy(x => x.ContestId)
            .ToDictionaryAsync(x => x.Key, x => x.ToList());
    }

    private static bool IsMainVotingCardsAttendee(ContestDomainOfInfluence mainVotingCardDoi, ContestDomainOfInfluence host)
    {
        return mainVotingCardDoi.ResponsibleForVotingCards
            && mainVotingCardDoi.IsMainVotingCardsDomainOfInfluence
            && mainVotingCardDoi.Id != host.Id
            && mainVotingCardDoi.Role != ContestRole.None
            && (mainVotingCardDoi.SecureConnectId == host.SecureConnectId
                || host.CountingCircles!.Any(doiCc => doiCc.CountingCircle!.SecureConnectId == mainVotingCardDoi.SecureConnectId));
    }

    private async Task EnsureAttachmentStepIsApproved(Guid domainOfInfluenceId)
    {
        if (!await _stepStateRepo.Query().AnyAsync(x => x.Approved && x.Step == Step.Attachments && x.DomainOfInfluenceId == domainOfInfluenceId))
        {
            throw new ValidationException("The attachment step is not approved yet.");
        }
    }
}
