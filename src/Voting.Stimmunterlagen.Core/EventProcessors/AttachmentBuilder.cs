// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class AttachmentBuilder
{
    private readonly DataContext _dataContext;

    public AttachmentBuilder(DataContext dataContext)
    {
        _dataContext = dataContext;
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
        var dois = await _dataContext.ContestDomainOfInfluences
            .AsTracking()
            .Where(x => x.BasisDomainOfInfluenceId == basisDoiId)
            .Include(x => x.Attachments)
            .Include(x => x.DomainOfInfluenceAttachmentCounts!).ThenInclude(x => x.Attachment)
            .Include(x => x.PoliticalBusinessPermissionEntries)
            .Include(x => x.HierarchyEntries)!
                .ThenInclude(x => x.ParentDomainOfInfluence!.Attachments!)
                    .ThenInclude(x => x.DomainOfInfluenceAttachmentCounts)
            .Include(x => x.Contest)
            .ToListAsync();

        foreach (var doi in dois)
        {
            SyncAttachments(doi);
            SyncDomainOfInfluenceAttachmentCounts(doi);
        }

        await _dataContext.SaveChangesAsync();
    }

    private void SyncDomainOfInfluenceAttachmentCounts(ContestDomainOfInfluence doi)
    {
        var responsibleForVotingCardsAndIsAnAttendee = doi.UsesVotingCardsInCurrentContest();

        foreach (var parentAttachment in doi.HierarchyEntries!.SelectMany(x => x.ParentDomainOfInfluence!.Attachments!))
        {
            var existingDomainOfInfluenceAttachmentCount = parentAttachment.DomainOfInfluenceAttachmentCounts!.FirstOrDefault(x => x.DomainOfInfluenceId == doi.Id);
            if (responsibleForVotingCardsAndIsAnAttendee && existingDomainOfInfluenceAttachmentCount == null)
            {
                parentAttachment.DomainOfInfluenceAttachmentCounts!.Add(new()
                {
                    DomainOfInfluenceId = doi.Id,
                });

                continue;
            }

            if (!responsibleForVotingCardsAndIsAnAttendee && existingDomainOfInfluenceAttachmentCount != null)
            {
                parentAttachment.DomainOfInfluenceAttachmentCounts!.Remove(existingDomainOfInfluenceAttachmentCount);
                AdjustAttachmentTotalCounts(parentAttachment, existingDomainOfInfluenceAttachmentCount);
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
}
