// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ContestManager
{
    private const int PrintingCenterSignUpDeadlineCommunalDeadlineDays = 31;
    private const int AttachmentDeliveryCommunalDeadlineDays = 22;
    private const int GenerateVotingCardsCommunalDeadlineDays = 24;

    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IAuth _auth;
    private readonly IClock _clock;
    private readonly IDbRepository<StepState> _stepStateRepo;
    private readonly DataContext _dbContext;

    public ContestManager(
        IAuth auth,
        IDbRepository<Contest> contestRepo,
        IClock clock,
        IDbRepository<StepState> stepStateRepo,
        DataContext dbContext)
    {
        _contestRepo = contestRepo;
        _auth = auth;
        _clock = clock;
        _stepStateRepo = stepStateRepo;
        _dbContext = dbContext;
    }

    public async Task SetDeadlines(
        Guid id,
        DateTime printingCenterSignUpDeadlineDate,
        DateTime attachmentDeliveryDeadlineDate,
        DateTime generateVotingCardsDeadlineDate,
        DateTime? electoralRegisterEVotingFromDate)
    {
        var now = _clock.UtcNow;

        // deadlines are stored as date with time informations.
        var printingCenterSignUpDeadline = printingCenterSignUpDeadlineDate.NextUtcDate(true);
        var attachmentDeliveryDeadline = attachmentDeliveryDeadlineDate.NextUtcDate(true);
        var generateVotingCardsDeadline = generateVotingCardsDeadlineDate.NextUtcDate(true);
        var electoralRegisterEVotingFrom = electoralRegisterEVotingFromDate?.NextUtcDate(true).AddDays(-1);

        if (printingCenterSignUpDeadline <= now
            || attachmentDeliveryDeadline <= now
            || generateVotingCardsDeadline <= now)
        {
            throw new ValidationException("Contest deadlines must not be in the past");
        }

        if (electoralRegisterEVotingFrom.HasValue && electoralRegisterEVotingFrom.Value.AddDays(1) <= now)
        {
            throw new ValidationException("Electoral register e-voting from must not be in the past");
        }

        EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(printingCenterSignUpDeadline, generateVotingCardsDeadline);

        var contest = await _contestRepo
                          .Query()
                          .WhereNotLocked()
                          .WhereIsContestManager(_auth.Tenant.Id)
                          .Where(c => c.DomainOfInfluence!.StepStates!.Any(s => s.Step == Step.ContestApproval && !s.Approved))
                          .Include(c => c.DomainOfInfluence)
                          .FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new EntityNotFoundException(nameof(Contest), id);

        if (contest.DomainOfInfluence!.Type.IsCommunal())
        {
            throw new ValidationException("Cannot set non-communal deadlines on communal contest");
        }

        if (contest.EVoting && (!electoralRegisterEVotingFrom.HasValue || generateVotingCardsDeadline <= electoralRegisterEVotingFrom.Value))
        {
            throw new ValidationException("Electoral register e-voting date is required for e-voting contest and must take place before the generate voting cards deadline");
        }

        if (!contest.EVoting && electoralRegisterEVotingFrom.HasValue)
        {
            throw new ValidationException("Cannot set electoral register e-voting date when contest supports no e-evoting");
        }

        contest.AttachmentDeliveryDeadline = attachmentDeliveryDeadline;
        contest.PrintingCenterSignUpDeadline = printingCenterSignUpDeadline;
        contest.GenerateVotingCardsDeadline = generateVotingCardsDeadline;
        contest.ElectoralRegisterEVotingFrom = electoralRegisterEVotingFrom;
        await _contestRepo.UpdateIgnoreRelations(contest);
    }

    public async Task<ContestCommunalDeadlinesCalculationResult> GetPreviewCommunalDeadlines(Guid id, DateTime deliveryToPostDeadlineDate)
    {
        var contest = await _contestRepo
            .Query()
            .WhereNotLocked()
            .WhereIsContestManager(_auth.Tenant.Id)
            .Where(c => c.DomainOfInfluence!.StepStates!.Any(s => s.Step == Step.ContestApproval && !s.Approved))
            .Include(c => c.DomainOfInfluence)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(Contest), id);

        return ValidateAndCalculateCommunalDeadlines(deliveryToPostDeadlineDate, contest);
    }

    public async Task<ContestCommunalDeadlinesCalculationResult> SetCommunalDeadlines(Guid id, DateTime deliveryToPostDeadlineDate)
    {
        var contest = await _contestRepo
                  .Query()
                  .WhereNotLocked()
                  .WhereIsContestManager(_auth.Tenant.Id)
                  .Where(c => c.DomainOfInfluence!.StepStates!.Any(s => s.Step == Step.ContestApproval && !s.Approved))
                  .Include(c => c.DomainOfInfluence)
                  .FirstOrDefaultAsync(x => x.Id == id)
              ?? throw new EntityNotFoundException(nameof(Contest), id);

        var calculationResult = ValidateAndCalculateCommunalDeadlines(deliveryToPostDeadlineDate, contest);
        contest.DeliveryToPostDeadline = calculationResult.DeliveryToPostDeadline;
        contest.PrintingCenterSignUpDeadline = calculationResult.PrintingCenterSignUpDeadline;
        contest.AttachmentDeliveryDeadline = calculationResult.AttachmentDeliveryDeadline;
        contest.GenerateVotingCardsDeadline = calculationResult.GenerateVotingCardsDeadline;
        await _contestRepo.UpdateIgnoreRelations(contest);
        return calculationResult;
    }

    public async Task<Contest> Get(Guid id, bool forPrintJobManager)
    {
        var query = _contestRepo.Query();

        if (!forPrintJobManager)
        {
            query = query.WhereHasAccessToContest(_auth.Tenant.Id);
        }

        return await query
                   .Include(x => x.DomainOfInfluence)
                   .Include(x => x.Translations)
                   .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new EntityNotFoundException(nameof(Contest), id);
    }

    public async Task<List<Contest>> List(IReadOnlyCollection<ContestState> states, bool forPrintJobManagement)
    {
        var tenantId = _auth.Tenant.Id;
        var query = _contestRepo.Query();

        if (states.Count > 0)
        {
            query = query.Where(x => states.Contains(x.State));
        }

        if (!forPrintJobManagement)
        {
            query = query
                .WhereHasAccessToContest(tenantId)
                .Where(c => c.DomainOfInfluence!.SecureConnectId == tenantId || c.Approved.HasValue);
        }
        else
        {
            query = query.Where(c => c.Approved.HasValue)
                .Include(c => c.ContestDomainOfInfluences!).ThenInclude(x => x.PrintJob);
        }

        return await query
            .Include(x => x.DomainOfInfluence)
            .Include(x => x.Translations)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task ResetGenerateVotingCardsAndUpdateContestDeadlines(
        Guid id,
        DateTime printingCenterSignUpDeadlineDate,
        DateTime generateVotingCardsDeadlineDate,
        IReadOnlyCollection<Guid> resetGenerateVotingCardsTriggeredDoiIds)
    {
        var now = _clock.UtcNow;

        // deadlines are stored as date with time informations.
        var printingCenterSignUpDeadline = printingCenterSignUpDeadlineDate.NextUtcDate(true);
        var generateVotingCardsDeadline = generateVotingCardsDeadlineDate.NextUtcDate(true);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var contest = await _contestRepo
                          .Query()
                          .Include(x => x.DomainOfInfluence)
                          .WhereNotLocked()
                          .WhereIsContestManager(_auth.Tenant.Id)
                          .FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new EntityNotFoundException(nameof(Contest), id);

        if (contest.DomainOfInfluence!.Type.IsCommunal())
        {
            throw new ValidationException("Cannot set non-communal deadlines on communal contest");
        }

        if (contest.PrintingCenterSignUpDeadline == null || contest.GenerateVotingCardsDeadline == null)
        {
            throw new ValidationException("Contest deadlines must be set before they can be updated");
        }

        if (printingCenterSignUpDeadline <= now
            || generateVotingCardsDeadline <= now)
        {
            throw new ValidationException("Contest deadlines must not be in the past");
        }

        EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(printingCenterSignUpDeadline, generateVotingCardsDeadline);

        if (contest.EVoting && contest.ElectoralRegisterEVotingFrom.HasValue && generateVotingCardsDeadline <= contest.ElectoralRegisterEVotingFrom.Value)
        {
            throw new ValidationException("Electoral register e-voting must take place before the generate voting cards deadline");
        }

        contest.PrintingCenterSignUpDeadline = printingCenterSignUpDeadline;
        contest.GenerateVotingCardsDeadline = generateVotingCardsDeadline;
        await _contestRepo.UpdateIgnoreRelations(contest);
        await ResetGenerateVotingCardsTriggered(id, resetGenerateVotingCardsTriggeredDoiIds);

        await transaction.CommitAsync();
    }

    public async Task ResetGenerateVotingCardsAndUpdateCommunalContestDeadlines(
        Guid id,
        DateTime printingCenterSignUpDeadlineDate,
        DateTime attachmentDeliveryDeadlineDate,
        DateTime generateVotingCardsDeadlineDate,
        DateTime deliveryToPostDateDate,
        IReadOnlyCollection<Guid> resetGenerateVotingCardsTriggeredDoiIds)
    {
        var now = _clock.UtcNow;

        // deadlines are stored as date with time informations.
        var printingCenterSignUpDeadline = printingCenterSignUpDeadlineDate.NextUtcDate(true);
        var attachmentDeliveryDeadline = attachmentDeliveryDeadlineDate.NextUtcDate(true);
        var generateVotingCardsDeadline = generateVotingCardsDeadlineDate.NextUtcDate(true);
        var deliveryToPostDeadline = deliveryToPostDateDate.NextUtcDate(true);

        if (printingCenterSignUpDeadline <= now
            || attachmentDeliveryDeadline <= now
            || generateVotingCardsDeadline <= now
            || deliveryToPostDeadline <= now)
        {
            throw new ValidationException("Contest deadlines must not be in the past");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var contest = await _contestRepo
            .Query()
            .Include(x => x.DomainOfInfluence)
            .WhereNotLocked()
            .FirstOrDefaultAsync(x => x.Id == id
                && x.DomainOfInfluence!.StepStates!.Any(s => s.Step == Step.ContestApproval && s.Approved))
            ?? throw new EntityNotFoundException(nameof(Contest), id);

        if (!contest.DomainOfInfluence!.Type.IsCommunal())
        {
            throw new ValidationException("Cannot set communal deadlines on non-communal contest");
        }

        EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(printingCenterSignUpDeadline, generateVotingCardsDeadline);

        contest.PrintingCenterSignUpDeadline = printingCenterSignUpDeadline;
        contest.AttachmentDeliveryDeadline = attachmentDeliveryDeadline;
        contest.GenerateVotingCardsDeadline = generateVotingCardsDeadline;
        contest.DeliveryToPostDeadline = deliveryToPostDeadline;

        await _contestRepo.UpdateIgnoreRelations(contest);
        await ResetGenerateVotingCardsTriggered(id, resetGenerateVotingCardsTriggeredDoiIds);

        await transaction.CommitAsync();
    }

    internal async Task EnsureIsContestManager(Guid contestId)
    {
        var hasAccess = await _contestRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .AnyAsync(x => x.Id == contestId);

        if (!hasAccess)
        {
            throw new ForbiddenException("No access to this contest");
        }
    }

    private async Task ResetGenerateVotingCardsTriggered(Guid contestId, IReadOnlyCollection<Guid> resetGenerateVotingCardsTriggeredDoiIds)
    {
        if (resetGenerateVotingCardsTriggeredDoiIds.Count == 0)
        {
            return;
        }

        var stepStates = await _stepStateRepo.Query()
            .Include(s => s.DomainOfInfluence!.PrintJob)
            .WhereGenerateVotingCardsTriggered()
            .WherePrintJobProcessNotStarted()
            .Where(s => s.Step == Step.GenerateVotingCards
                        && s.DomainOfInfluence!.ContestId == contestId
                        && resetGenerateVotingCardsTriggeredDoiIds.Contains(s.DomainOfInfluenceId))
            .ToListAsync();

        if (resetGenerateVotingCardsTriggeredDoiIds.Count != stepStates.Count)
        {
            throw new ValidationException("Some passed domain of influences have no generated voting cards or their print job processing already started or are not in the same contest");
        }

        foreach (var stepState in stepStates)
        {
            stepState.Approved = false;
            stepState.DomainOfInfluence!.GenerateVotingCardsTriggered = null;

            // submission on going, because the pbs are already approved
            // (because we only include dois where generate voting cards are already triggered)
            stepState.DomainOfInfluence!.PrintJob!.State = PrintJobState.SubmissionOngoing;
        }

        await _stepStateRepo.UpdateRange(stepStates);
    }

    private void EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(
        DateTime printingCenterSignupDeadline,
        DateTime generateVotingCardsDeadline)
    {
        if (printingCenterSignupDeadline > generateVotingCardsDeadline)
        {
            throw new ValidationException("Printing center sign up deadline must take place earlier or at the same date than generate voting cards deadline");
        }
    }

    private ContestCommunalDeadlinesCalculationResult ValidateAndCalculateCommunalDeadlines(DateTime deliveryToPostDeadlineDate, Contest contest)
    {
        var now = _clock.UtcNow;

        // deadlines are stored as date with time informations.
        var deliveryToPostDeadline = deliveryToPostDeadlineDate.NextUtcDate(true);

        if (deliveryToPostDeadline <= now)
        {
            throw new ValidationException("Contest deadlines must not be in the past");
        }

        if (!contest.DomainOfInfluence!.Type.IsCommunal())
        {
            throw new ValidationException("Cannot calculate communal deadlines on non-communal contest");
        }

        if (contest.Date <= deliveryToPostDeadline)
        {
            throw new ValidationException("Communal delivery to post deadline must be set earlier than the contest date");
        }

        var printingCenterSignUpDeadline = deliveryToPostDeadline.AddDays(-PrintingCenterSignUpDeadlineCommunalDeadlineDays);
        var attachmentDeliveryDeadline = deliveryToPostDeadline.AddDays(-AttachmentDeliveryCommunalDeadlineDays);
        var generateVotingCardsDeadline = deliveryToPostDeadline.AddDays(-GenerateVotingCardsCommunalDeadlineDays);
        return new(printingCenterSignUpDeadline, attachmentDeliveryDeadline, generateVotingCardsDeadline, deliveryToPostDeadline);
    }
}
