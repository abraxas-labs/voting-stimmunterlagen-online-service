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

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class AttachmentBuilder
{
    private readonly DataContext _dataContext;
    private readonly DomainOfInfluenceManager _domainOfInfluenceManager;

    public AttachmentBuilder(DataContext dataContext, DomainOfInfluenceManager domainOfInfluenceManager)
    {
        _dataContext = dataContext;
        _domainOfInfluenceManager = domainOfInfluenceManager;
    }

    internal async Task CleanUp(IReadOnlyCollection<Guid> contestIds)
    {
        var attachmentsToDelete = await _dataContext.Attachments
            .Where(a => contestIds.Contains(a.DomainOfInfluence!.ContestId) &&
                ((!a.DomainOfInfluence!.Contest!.IsPoliticalAssembly && a.PoliticalBusinessEntries.Count == 0) || a.DomainOfInfluence!.StepStates!.Count(s => s.Step == Step.Attachments) == 0))
            .ToListAsync();

        _dataContext.Attachments.RemoveRange(attachmentsToDelete);
        await _dataContext.SaveChangesAsync();
    }

    internal async Task SyncForBasisDomainOfInfluence(Guid basisDoiId)
    {
        var dois = await LoadDomainOfInfluencesForBasis(basisDoiId);
        if (dois.Count == 0)
        {
            return;
        }

        var contestIds = dois.Select(d => d.ContestId).Distinct().ToList();
        var mainVotingCardsDoisByContestId = await _domainOfInfluenceManager.GetMainVotingCardsDomainOfInfluencesByContestId(contestIds);

        var hostsByDoiId = BuildHostsByDoiId(
            dois,
            contestIds,
            await LoadRelatedContestDomainOfInfluencesByContestId(contestIds),
            mainVotingCardsDoisByContestId);

        await LoadAttachmentsForDomainOfInfluences(hostsByDoiId.Values.SelectMany(v => v).Distinct().ToList());

        foreach (var doi in dois)
        {
            SyncAttachments(doi);

            var mainVotingCardsDois = mainVotingCardsDoisByContestId.GetValueOrDefault(doi.ContestId) ?? new();
            var pbAttendees = _domainOfInfluenceManager.ListPoliticalBusinessAttendees(doi, mainVotingCardsDois);
            SyncDomainOfInfluenceAttachmentCounts(doi, hostsByDoiId[doi.Id], pbAttendees);
        }

        await _dataContext.SaveChangesAsync();
    }

    private void SyncDomainOfInfluenceAttachmentCounts(
        ContestDomainOfInfluence domainOfInfluence,
        List<ContestDomainOfInfluence> hosts,
        List<ContestDomainOfInfluence> pbAttendees)
    {
        var doiIsPbAttendee = domainOfInfluence.UsesVotingCardsInCurrentContest();

        // Affects host attachments (host doi is a parent or a related doi (if the current doi is a main doi)).
        // e.g. Adds a DoiAc if the current doi is now newly an "attendee" of a host doi.
        // e.g. Removes a DoiAc if the current doi is not an "attendee" anymore of a host Doi
        foreach (var hostAttachment in hosts.SelectMany(x => x.Attachments!))
        {
            var existingDomainOfInfluenceAttachmentCount = hostAttachment.DomainOfInfluenceAttachmentCounts!.FirstOrDefault(x => x.DomainOfInfluenceId == domainOfInfluence.Id);
            if (doiIsPbAttendee && existingDomainOfInfluenceAttachmentCount == null)
            {
                hostAttachment.DomainOfInfluenceAttachmentCounts!.Add(new()
                {
                    DomainOfInfluenceId = domainOfInfluence.Id,
                });

                continue;
            }

            if (!doiIsPbAttendee && existingDomainOfInfluenceAttachmentCount != null)
            {
                hostAttachment.DomainOfInfluenceAttachmentCounts!.Remove(existingDomainOfInfluenceAttachmentCount);
                AdjustAttachmentTotalCounts(hostAttachment, existingDomainOfInfluenceAttachmentCount);
            }
        }

        var pbAttendeeIds = pbAttendees.ConvertAll(doi => doi.Id).ToHashSet();

        // Affects owned attachments, adds or removes if the dois have a pb attendee change.
        foreach (var attachment in domainOfInfluence.Attachments!)
        {
            var attachmentDoiAcByDoiId = attachment.DomainOfInfluenceAttachmentCounts!.ToDictionary(x => x.DomainOfInfluenceId);

            foreach (var existingAttachmentDoiId in attachmentDoiAcByDoiId.Keys)
            {
                if (existingAttachmentDoiId == domainOfInfluence.Id || pbAttendeeIds.Contains(existingAttachmentDoiId))
                {
                    continue;
                }

                attachment.DomainOfInfluenceAttachmentCounts!.Remove(attachmentDoiAcByDoiId[existingAttachmentDoiId]);
                AdjustAttachmentTotalCounts(attachment, attachmentDoiAcByDoiId[existingAttachmentDoiId]);
            }

            foreach (var pbAttendeeId in pbAttendeeIds)
            {
                if (attachmentDoiAcByDoiId.ContainsKey(pbAttendeeId))
                {
                    continue;
                }

                attachment.DomainOfInfluenceAttachmentCounts!.Add(new()
                {
                    DomainOfInfluenceId = pbAttendeeId,
                });
            }
        }
    }

    private void SyncAttachments(ContestDomainOfInfluence doi)
    {
        if (!doi.ExternalPrintingCenter)
        {
            return;
        }

        // a doi with external printing center never owns attachments
        // but it can set domain of influence attachment counts as an political business attendee.
        doi.Attachments?.Clear();
    }

    private void AdjustAttachmentTotalCounts(Attachment attachment, DomainOfInfluenceAttachmentCount domainOfInfluenceAttachmentCount)
    {
        attachment!.TotalRequiredCount -= domainOfInfluenceAttachmentCount.RequiredCount.GetValueOrDefault();
        attachment!.TotalRequiredForVoterListsCount -= domainOfInfluenceAttachmentCount.RequiredForVoterListsCount;
    }

    private async Task LoadAttachmentsForDomainOfInfluences(List<ContestDomainOfInfluence> domainOfInfluences)
    {
        var doiIds = domainOfInfluences.ConvertAll(h => h.Id);
        var attachmentsByDoiId = await _dataContext
            .Attachments
            .AsTracking()
            .Include(a => a.DomainOfInfluenceAttachmentCounts)
            .Where(a => doiIds.Contains(a.DomainOfInfluenceId))
            .GroupBy(a => a.DomainOfInfluenceId)
            .ToDictionaryAsync(x => x.Key, x => x.ToList());

        foreach (var doi in domainOfInfluences)
        {
            doi.Attachments = attachmentsByDoiId.GetValueOrDefault(doi.Id) ?? new List<Attachment>();
        }
    }

    private async Task<List<ContestDomainOfInfluence>> LoadDomainOfInfluencesForBasis(Guid basisDoiId)
    {
        return await _dataContext.ContestDomainOfInfluences
            .AsSplitQuery()
            .AsTracking()
            .Where(x => x.BasisDomainOfInfluenceId == basisDoiId)
            .WhereContestInTestingPhase()
            .Include(x => x.CountingCircles!).ThenInclude(x => x.CountingCircle!)
            .Include(x => x.Attachments!).ThenInclude(x => x.DomainOfInfluenceAttachmentCounts)
            .Include(x => x.DomainOfInfluenceAttachmentCounts!).ThenInclude(x => x.Attachment)
            .Include(x => x.PoliticalBusinessPermissionEntries)
            .Include(x => x.HierarchyEntries)!
                .ThenInclude(x => x.ParentDomainOfInfluence)
            .Include(x => x.ParentHierarchyEntries!)
                .ThenInclude(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries)
            .Include(x => x.Contest)
            .ToListAsync();
    }

    private async Task<Dictionary<Guid, List<ContestDomainOfInfluence>>> LoadRelatedContestDomainOfInfluencesByContestId(List<Guid> contestIds)
    {
        return await _dataContext.ContestDomainOfInfluences
            .WhereContestInTestingPhase()
            .Where(doi => doi.Role != ContestRole.None && contestIds.Contains(doi.ContestId))
            .Include(x => x.CountingCircles!)
                .ThenInclude(x => x.CountingCircle)
            .GroupBy(doi => doi.ContestId)
            .ToDictionaryAsync(x => x.Key, x => x.ToList());
    }

    private Dictionary<Guid, List<ContestDomainOfInfluence>> BuildHostsByDoiId(
        List<ContestDomainOfInfluence> dois,
        List<Guid> contestIds,
        Dictionary<Guid, List<ContestDomainOfInfluence>> relatedContestDomainOfInfluencesByContestId,
        Dictionary<Guid, List<ContestDomainOfInfluence>> mainVotingCardsDoisByContestId)
    {
        var hostsByDoiId = new Dictionary<Guid, List<ContestDomainOfInfluence>>();
        foreach (var contestId in contestIds)
        {
            var attendees = dois.Where(d => d.ContestId == contestId).ToList();
            var contestDois = relatedContestDomainOfInfluencesByContestId.GetValueOrDefault(contestId) ?? new();
            var mainVotingCardsDois = mainVotingCardsDoisByContestId.GetValueOrDefault(contestId) ?? new();

            foreach (var (doiId, hosts) in _domainOfInfluenceManager.BuildHostsByAttendeeId(attendees, contestDois, mainVotingCardsDois))
            {
                hostsByDoiId[doiId] = hosts;
            }
        }

        return hostsByDoiId;
    }
}
