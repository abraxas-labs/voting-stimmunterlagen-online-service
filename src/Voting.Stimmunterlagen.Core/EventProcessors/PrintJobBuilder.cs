// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class PrintJobBuilder
{
    private readonly DataContext _context;
    private readonly IDbRepository<ContestDomainOfInfluence> _contestDomainOfInfluenceRepo;

    public PrintJobBuilder(
        DataContext context,
        IDbRepository<ContestDomainOfInfluence> contestDomainOfInfluenceRepo)
    {
        _context = context;
        _contestDomainOfInfluenceRepo = contestDomainOfInfluenceRepo;
    }

    internal Task SyncForBasisDomainOfInfluence(Guid basisDoiId) => Sync(x => x.BasisDomainOfInfluenceId == basisDoiId);

    internal Task SyncForContest(Guid contestId) => Sync(x => x.ContestId == contestId);

    internal async Task SyncStateForDomainOfInfluence(Guid doiId, Dictionary<Guid, List<Attachment>> allRequiredAttachmentsByDoiId)
    {
        var doi = await _contestDomainOfInfluenceRepo
            .Query()
            .AsTracking()
            .Include(x => x.PrintJob)
            .Include(x => x.ParentHierarchyEntries!.Where(y => y.DomainOfInfluence!.PrintJob!.State >= PrintJobState.SubmissionOngoing))
                .ThenInclude(x => x.DomainOfInfluence)
                .ThenInclude(x => x!.PrintJob)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(Attachment), doiId);

        var affectedDois = doi.ParentHierarchyEntries!.Select(x => x.DomainOfInfluence!)
            .ToList();
        affectedDois.Add(doi);

        foreach (var affectedDoi in affectedDois)
        {
            if (affectedDoi.PrintJob == null || affectedDoi.PrintJob.ProcessStarted())
            {
                continue;
            }

            var attachments = allRequiredAttachmentsByDoiId[affectedDoi.Id];
            affectedDoi.PrintJob!.State = attachments.All(x => x.State >= AttachmentState.Delivered) && affectedDoi.GenerateVotingCardsTriggered.HasValue
                ? PrintJobState.ReadyForProcess
                : PrintJobState.SubmissionOngoing;
        }

        await _context.SaveChangesAsync();
    }

    private async Task Sync(Expression<Func<ContestDomainOfInfluence, bool>> predicate)
    {
        var dois = await _context.ContestDomainOfInfluences
            .Include(x => x.PrintJob)
            .Include(x => x.PoliticalBusinessPermissionEntries)
            .Include(x => x.DomainOfInfluenceAttachmentCounts!)
                .ThenInclude(x => x.Attachment)
            .Include(x => x.Contest)
            .AsTracking()
            .Where(predicate)
            .ToListAsync();

        foreach (var doi in dois)
        {
            SyncPrintJob(doi);
        }

        await _context.SaveChangesAsync();
    }

    private void SyncPrintJob(ContestDomainOfInfluence doi)
    {
        if (!doi.UsesVotingCardsInCurrentContest() || doi.ExternalPrintingCenter)
        {
            doi.PrintJob = null;
            return;
        }

        if (doi.PrintJob != null)
        {
            // ensures that if a pb or doi is removed with its attachments,
            // that the print job can then be processed if all current attachments are delivered.
            if (doi.PrintJob.State == PrintJobState.SubmissionOngoing
                && doi.GenerateVotingCardsTriggered.HasValue
                && doi.DomainOfInfluenceAttachmentCounts!.All(doiAc => doiAc.Attachment!.State >= AttachmentState.Delivered))
            {
                doi.PrintJob.State = PrintJobState.ReadyForProcess;
            }

            return;
        }

        doi.PrintJob = new()
        {
            State = PrintJobState.Empty,
        };
    }
}
