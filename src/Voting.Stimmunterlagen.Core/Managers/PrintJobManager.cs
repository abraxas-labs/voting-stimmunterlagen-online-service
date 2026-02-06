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
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Data.Utils;

namespace Voting.Stimmunterlagen.Core.Managers;

public class PrintJobManager
{
    private readonly IDbRepository<ContestDomainOfInfluence> _contestDomainOfInfluenceRepo;
    private readonly DataContext _dbContext;
    private readonly IDbRepository<PrintJob> _printJobRepo;
    private readonly AttachmentManager _attachmentManager;
    private readonly IClock _clock;
    private readonly IAuth _auth;
    private readonly VotingCardPrintFileExportJobBuilder _votingCardPrintFileExportJobBuilder;
    private readonly VotingCardPrintFileExportJobLauncher _votingCardPrintFileExportJobLauncher;

    public PrintJobManager(
        IDbRepository<PrintJob> printJobRepo,
        AttachmentManager attachmentManager,
        IClock clock,
        IAuth auth,
        VotingCardPrintFileExportJobBuilder votingCardPrintFileExportJobBuilder,
        VotingCardPrintFileExportJobLauncher votingCardPrintFileExportJobLauncher,
        IDbRepository<ContestDomainOfInfluence> contestDomainOfInfluenceRepo,
        DataContext dbContext)
    {
        _printJobRepo = printJobRepo;
        _attachmentManager = attachmentManager;
        _clock = clock;
        _auth = auth;
        _votingCardPrintFileExportJobBuilder = votingCardPrintFileExportJobBuilder;
        _votingCardPrintFileExportJobLauncher = votingCardPrintFileExportJobLauncher;
        _contestDomainOfInfluenceRepo = contestDomainOfInfluenceRepo;
        _dbContext = dbContext;
    }

    public async Task<PrintJob> Get(Guid domainOfInfluenceId, bool forPrintJobManagement)
    {
        var query = _printJobRepo.Query();

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        return await query
                   .Include(x => x.DomainOfInfluence)
                   .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == domainOfInfluenceId)
               ?? throw new EntityNotFoundException(nameof(PrintJob), domainOfInfluenceId);
    }

    public async Task<List<PrintJobSummary>> List(Guid contestId, bool forPrintJobManagement, string queryString = "", PrintJobState? state = null)
    {
        var escapedLikeQuery = $"%{SqlUtils.EscapeLike(queryString)}%";

        var query = _printJobRepo.Query();

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        var printJobs = await query
            .Where(x => x.DomainOfInfluence!.ContestId == contestId
                && (state == null || x.State == state)
                && (string.IsNullOrEmpty(queryString)
                    || EF.Functions.ILike(x.DomainOfInfluence.Name, escapedLikeQuery, SqlUtils.DefaultEscapeCharacter)
                    || EF.Functions.ILike(x.DomainOfInfluence.AuthorityName, escapedLikeQuery, SqlUtils.DefaultEscapeCharacter)))
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .Include(x => x.DomainOfInfluence)
            .ToListAsync();

        return await MapToPrintJobSummaries(contestId, printJobs);
    }

    public async Task<List<PrintJob>> ListGenerateVotingCardsTriggered(Guid contestId, bool forPrintJobManagement)
    {
        var query = _printJobRepo.Query()
            .WhereGenerateVotingCardsTriggered()
            .WherePrintJobProcessNotStarted()
            .Where(x => x.DomainOfInfluence!.ContestId == contestId);

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        return await query
            .Include(x => x.DomainOfInfluence)
            .OrderBy(x => x.DomainOfInfluence!.Name)
            .ToListAsync();
    }

    public async Task ResetState(Guid domainOfInfluenceId)
    {
        var existingPrintJob = await LoadPrintJob(domainOfInfluenceId);

        var currentState = existingPrintJob.State;

        if (currentState <= PrintJobState.SubmissionOngoing)
        {
            throw new ValidationException($"reset state on state {currentState} is not allowed");
        }

        var newState = currentState - 1;

        switch (newState)
        {
            case PrintJobState.ReadyForProcess:
                existingPrintJob.ProcessStartedOn = null;
                break;
            case PrintJobState.ProcessStarted:
                existingPrintJob.ProcessEndedOn = null;
                existingPrintJob.VotingCardsPrintedAndPackedCount = 0;
                existingPrintJob.VotingCardsShipmentWeight = 0;
                break;
            case PrintJobState.ProcessEnded:
                existingPrintJob.DoneOn = null;
                existingPrintJob.DoneComment = string.Empty;
                break;
            default:
                throw new ValidationException($"state {newState} is invalid");
        }

        existingPrintJob.State = newState;
        await _printJobRepo.Update(existingPrintJob);
    }

    public async Task SetProcessStarted(Guid domainOfInfluenceId)
    {
        List<VotingCardPrintFileExportJob> jobs;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var existingPrintJob = await LoadPrintJob(domainOfInfluenceId);

        if (existingPrintJob.State != PrintJobState.ReadyForProcess)
        {
            throw new ValidationException($"cannot start the process of the print job, because it has the wrong state: {existingPrintJob.State}");
        }

        jobs = await _votingCardPrintFileExportJobBuilder.CleanAndBuildJobs(domainOfInfluenceId);

        existingPrintJob.State = PrintJobState.ProcessStarted;
        existingPrintJob.ProcessStartedOn = _clock.UtcNow;
        await _printJobRepo.Update(existingPrintJob);
        await transaction.CommitAsync();

        _ = _votingCardPrintFileExportJobLauncher.RunJobs(jobs.Select(x => x.Id));
    }

    public async Task SetProcessEnded(Guid domainOfInfluenceId, int votingCardsPrintedAndPackedCount, double votingCardsShipmentWeight)
    {
        var existingPrintJob = await LoadPrintJob(domainOfInfluenceId);

        if (existingPrintJob.State != PrintJobState.ProcessStarted)
        {
            throw new ValidationException($"cannot end the process of the print job, because it has the wrong state: {existingPrintJob.State}");
        }

        existingPrintJob.State = PrintJobState.ProcessEnded;
        existingPrintJob.VotingCardsPrintedAndPackedCount = votingCardsPrintedAndPackedCount;
        existingPrintJob.VotingCardsShipmentWeight = votingCardsShipmentWeight;
        existingPrintJob.ProcessEndedOn = _clock.UtcNow;
        await _printJobRepo.Update(existingPrintJob);
    }

    public async Task SetDone(Guid domainOfInfluenceId, string comment)
    {
        var existingPrintJob = await LoadPrintJob(domainOfInfluenceId);

        if (existingPrintJob.State != PrintJobState.ProcessEnded)
        {
            throw new ValidationException($"cannot set print job done, because it has the wrong state: {existingPrintJob.State}");
        }

        existingPrintJob.State = PrintJobState.Done;
        existingPrintJob.DoneComment = comment;
        existingPrintJob.DoneOn = _clock.UtcNow;
        await _printJobRepo.Update(existingPrintJob);
    }

    private async Task<PrintJob> LoadPrintJob(Guid domainOfInfluenceId)
    {
        return await _printJobRepo.Query()
            .Include(x => x.DomainOfInfluence)
            .WhereContestIsNotLocked()
            .WhereDomainOfInfluenceIsNotExternalPrintingCenter()
            .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == domainOfInfluenceId)
            ?? throw new EntityNotFoundException(nameof(PrintJob), domainOfInfluenceId);
    }

    private async Task<List<PrintJobSummary>> MapToPrintJobSummaries(Guid contestId, List<PrintJob> printJobs)
    {
        var summaries = new List<PrintJobSummary>();

        if (printJobs.Count == 0)
        {
            return summaries;
        }

        var doiIds = printJobs.Select(p => p.DomainOfInfluenceId).ToList();

        var domainOfInfluences = await _contestDomainOfInfluenceRepo.Query()
            .Include(x => x.StepStates!.Where(s => s.Approved))
            .Include(x => x.VotingCardLayouts!.Where(vc => vc.VotingCardType == VotingCardType.Swiss))
                .ThenInclude(x => x.Template)
            .Include(x => x.VotingCardLayouts!)
                .ThenInclude(x => x.DomainOfInfluenceTemplate)
            .Include(x => x.VotingCardLayouts!)
                .ThenInclude(x => x.OverriddenTemplate)
            .Include(x => x.PoliticalBusinessPermissionEntries!)
                .ThenInclude(x => x.PoliticalBusiness!.DomainOfInfluence)
            .Where(x => doiIds.Contains(x.Id))
            .ToListAsync();

        var doiById = domainOfInfluences.ToDictionary(x => x.Id);

        var allRequiredAttachmentsByDoiId = await _attachmentManager.GetAllRequiredAttachmentsByDomainOfInfluenceId(contestId);
        foreach (var printJob in printJobs)
        {
            var attachments = allRequiredAttachmentsByDoiId[printJob.DomainOfInfluenceId];
            var doi = doiById[printJob.DomainOfInfluenceId];

            summaries.Add(new PrintJobSummary(
                printJob,
                attachments.Count(a => a.State is AttachmentState.Defined or AttachmentState.Delivered),
                attachments.Count(a => a.State == AttachmentState.Delivered),
                doi.PoliticalBusinessPermissionEntries!.Any(pb => pb.DomainOfInfluence!.Type.IsCommunal()),
                doi.VotingCardLayouts!.FirstOrDefault()?.EffectiveTemplate?.Name ?? string.Empty,
                doi.StepStates!.MaxBy(x => x.Step)?.Step ?? Step.Unspecified));
        }

        return summaries;
    }
}
