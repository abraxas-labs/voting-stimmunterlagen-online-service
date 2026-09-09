// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class PoliticalBusinessPermissionBuilder
{
    private readonly DataContext _dbContext;
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _domainOfInfluenceRepo;
    private readonly DomainOfInfluenceManager _doiManager;

    public PoliticalBusinessPermissionBuilder(
        DataContext dbContext,
        IDbRepository<PoliticalBusiness> politicalBusinessRepo,
        IDbRepository<Contest> contestRepo,
        IDbRepository<ContestDomainOfInfluence> domainOfInfluenceRepo,
        DomainOfInfluenceManager doiManager)
    {
        _dbContext = dbContext;
        _politicalBusinessRepo = politicalBusinessRepo;
        _contestRepo = contestRepo;
        _domainOfInfluenceRepo = domainOfInfluenceRepo;
        _doiManager = doiManager;
    }

    internal async Task UpdatePermissionsForPoliticalBusinessesInTestingPhase()
    {
        IQueryable<PoliticalBusiness> query = _politicalBusinessRepo.Query()
            .AsTracking()
            .Include(x => x.Contest)
            .Include(x => x.PermissionEntries)
            .Include(x => x.DomainOfInfluence!.ParentHierarchyEntries!).ThenInclude(x => x.DomainOfInfluence)
            .Include(x => x.DomainOfInfluence!.CountingCircles!).ThenInclude(x => x.CountingCircle);

        var politicalBusinesses = await query
            .WhereContestInTestingPhase()
            .ToListAsync();

        if (politicalBusinesses.Count == 0)
        {
            return;
        }

        var mainVotingCardsDoisByContestId = await _doiManager.GetMainVotingCardsDomainOfInfluencesByContestId();

        foreach (var politicalBusiness in politicalBusinesses)
        {
            UpdatePermissionsForPoliticalBusiness(politicalBusiness, mainVotingCardsDoisByContestId.GetValueOrDefault(politicalBusiness.ContestId) ?? new());
        }

        // save changes, to have up to date data for the single attendee query
        await _dbContext.SaveChangesAsync();

        await UpdateIsSingleAttendeeForContest(politicalBusinesses.Select(x => x.Contest!).ToHashSet());
        await _dbContext.SaveChangesAsync();
    }

    internal async Task UpdatePermissionsForPoliticalBusiness(Guid id)
    {
        var politicalBusiness = await _politicalBusinessRepo.Query()
            .AsTracking()
            .Include(x => x.Contest)
            .Include(x => x.PermissionEntries)
            .Include(x => x.DomainOfInfluence!.ParentHierarchyEntries!).ThenInclude(x => x.DomainOfInfluence)
            .Include(x => x.DomainOfInfluence!.CountingCircles!).ThenInclude(x => x.CountingCircle)
            .FirstAsync(x => x.Id == id);

        var mainVotingCardsDoisByContestId = await _doiManager.GetMainVotingCardsDomainOfInfluencesByContestId(politicalBusiness.ContestId);
        UpdatePermissionsForPoliticalBusiness(politicalBusiness, mainVotingCardsDoisByContestId.GetValueOrDefault(politicalBusiness.ContestId) ?? new());

        // save changes, to have up to date data for the single attendee query
        await _dbContext.SaveChangesAsync();

        await UpdateIsSingleAttendeeForContest(new[] { politicalBusiness.Contest! });
        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateIsSingleAttendeeForContest(IReadOnlyCollection<Contest> contests)
    {
        var contestIds = contests.Select(x => x.Id).ToHashSet();
        var isSingleAttendeeByContestId = await _contestRepo.Query()
            .Where(x => contestIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                IsSingleAttendee = x.PoliticalBusinesses!
                    .SelectMany(pb => pb.PermissionEntries!)
                    .Where(y => y.Role == PoliticalBusinessRole.Attendee)
                    .Select(y => y.DomainOfInfluenceId)
                    .Distinct()
                    .Take(2) // limit to 2, since we are only interested if there is more than one or exactly one
                    .Count() <= 1, // <= 1 generates a much simpler sql than == 1, in our use case count() == 0 should never happen anyway.
            })
            .ToDictionaryAsync(x => x.Id, x => x.IsSingleAttendee);

        foreach (var contest in contests)
        {
            contest.IsSingleAttendeeContest = isSingleAttendeeByContestId.GetValueOrDefault(contest.Id);
        }
    }

    private void UpdatePermissionsForPoliticalBusiness(
        PoliticalBusiness politicalBusiness,
        List<ContestDomainOfInfluence> mainVotingCardsDomainOfInfluences)
    {
        var pbSecureConnectId = politicalBusiness.DomainOfInfluence!.SecureConnectId;

        var attendees = _doiManager
            .ListAttendees(politicalBusiness.DomainOfInfluence, mainVotingCardsDomainOfInfluences);

        politicalBusiness.PermissionEntries!.Clear();
        politicalBusiness.PermissionEntries.Add(new PoliticalBusinessPermissionEntry
        {
            DomainOfInfluenceId = politicalBusiness.DomainOfInfluenceId,
            SecureConnectId = pbSecureConnectId,
            Role = PoliticalBusinessRole.Manager,
        });

        if (politicalBusiness.DomainOfInfluence.ResponsibleForVotingCards)
        {
            politicalBusiness.PermissionEntries.Add(new PoliticalBusinessPermissionEntry
            {
                DomainOfInfluenceId = politicalBusiness.DomainOfInfluenceId,
                SecureConnectId = pbSecureConnectId,
                Role = PoliticalBusinessRole.Attendee,
            });
        }

        politicalBusiness.PermissionEntries.AddRange(attendees.Select(x => new PoliticalBusinessPermissionEntry
        {
            SecureConnectId = x.SecureConnectId,
            DomainOfInfluenceId = x.Id,
            Role = PoliticalBusinessRole.Attendee,
        }));
    }
}
