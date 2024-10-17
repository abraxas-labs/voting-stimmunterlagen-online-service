// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Invoice;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class InvoiceExportManager
{
    private readonly InvoiceFileBuilder _invoiceFileBuilder;
    private readonly IDbRepository<PrintJob> _printJobRepo;
    private readonly IDbRepository<Contest> _contestRepo;

    public InvoiceExportManager(
        InvoiceFileBuilder invoiceFileBuilder,
        IDbRepository<PrintJob> printJobRepo,
        IDbRepository<Contest> contestRepo)
    {
        _invoiceFileBuilder = invoiceFileBuilder;
        _printJobRepo = printJobRepo;
        _contestRepo = contestRepo;
    }

    public async Task<FileModel> GenerateExport(Guid contestId)
    {
        var contest = await _contestRepo.GetByKey(contestId)
            ?? throw new EntityNotFoundException(nameof(Contest), contestId);

        var printJobs = await _printJobRepo.Query()
            .AsSplitQuery()
            .WhereIsInContest(contestId)
            .Include(p => p.DomainOfInfluence!.DomainOfInfluenceAttachmentCounts!
                .Where(doiAc => doiAc.Attachment!.State == AttachmentState.Delivered && doiAc.RequiredForVoterListsCount > 0))
                .ThenInclude(doiAc => doiAc.Attachment!.PoliticalBusinessEntries)
            .Include(p => p.DomainOfInfluence!.VoterLists!)
                .ThenInclude(vl => vl.PoliticalBusinessEntries)
            .Include(p => p.DomainOfInfluence!.AdditionalInvoicePositions!.OrderBy(p => p.MaterialNumber))
            .OrderBy(p => p.DomainOfInfluence!.Name)
            .WhereDomainOfInfluenceIsNotExternalPrintingCenter()
            .ToListAsync();

        if (printJobs.Count == 0)
        {
            throw new ValidationException($"Cannot generate export for contest {contestId} because no print job exists");
        }

        var invoiceContent = await _invoiceFileBuilder.BuildInvoiceFile(printJobs);
        return new FileModel(invoiceContent, BuildFileName(contest), MimeTypes.CsvMimeType);
    }

    private string BuildFileName(Contest contest)
    {
        return $"Invoice_{contest.Date:yyyy_MM_dd}.csv";
    }
}
