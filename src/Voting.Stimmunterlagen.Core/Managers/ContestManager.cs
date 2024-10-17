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
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ContestManager
{
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
        DateTime generateVotingCardsDeadlineDate)
    {
        var now = _clock.UtcNow;

        // deadlines are stored as date with time informations.
        var printingCenterSignUpDeadline = printingCenterSignUpDeadlineDate.NextUtcDate(true);
        var attachmentDeliveryDeadline = attachmentDeliveryDeadlineDate.NextUtcDate(true);
        var generateVotingCardsDeadline = generateVotingCardsDeadlineDate.NextUtcDate(true);

        if (printingCenterSignUpDeadline <= now)
        {
            throw new ValidationException("Printing center sign up deadline must not be in the past");
        }

        if (attachmentDeliveryDeadline <= now)
        {
            throw new ValidationException("Attachment delivery deadline must not be in the past");
        }

        EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(printingCenterSignUpDeadline, generateVotingCardsDeadline);

        var contest = await _contestRepo
                          .Query()
                          .WhereNotLocked()
                          .WhereIsContestManager(_auth.Tenant.Id)
                          .Where(c => c.DomainOfInfluence!.StepStates!.Any(s => s.Step == Step.ContestApproval && !s.Approved))
                          .FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new EntityNotFoundException(nameof(Contest), id);
        contest.AttachmentDeliveryDeadline = attachmentDeliveryDeadline;
        contest.PrintingCenterSignUpDeadline = printingCenterSignUpDeadline;
        contest.GenerateVotingCardsDeadline = generateVotingCardsDeadline;
        await _contestRepo.Update(contest);
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
            query = query.Where(c => c.Approved.HasValue);
        }

        return await query
            .Include(x => x.DomainOfInfluence)
            .Include(x => x.Translations)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task UpdatePrintingCenterSignUpDeadline(
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
                          .WhereNotLocked()
                          .WhereIsContestManager(_auth.Tenant.Id)
                          .FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new EntityNotFoundException(nameof(Contest), id);

        if (contest.PrintingCenterSignUpDeadline == null || contest.GenerateVotingCardsDeadline == null)
        {
            throw new ValidationException("Contest deadlines must be set before they can be updated");
        }

        if (printingCenterSignUpDeadline <= now)
        {
            throw new ValidationException("Printing center sign up deadline must not be in the past");
        }

        EnsurePrintingCenterSignupDeadlineEarlierOrEqualGenerateVotingCardsDeadline(printingCenterSignUpDeadline, generateVotingCardsDeadline);

        contest.PrintingCenterSignUpDeadline = printingCenterSignUpDeadline;
        contest.GenerateVotingCardsDeadline = generateVotingCardsDeadline;
        await _contestRepo.Update(contest);
        await ResetGenerateVotingCardsTriggered(id, resetGenerateVotingCardsTriggeredDoiIds);

        await transaction.CommitAsync();
    }

    internal async Task EnsureHasAccessToContest(Guid contestId)
    {
        var hasAccess = await _contestRepo.Query()
            .WhereHasAccessToContest(_auth.Tenant.Id)
            .AnyAsync(x => x.Id == contestId);

        if (!hasAccess)
        {
            throw new ForbiddenException("No access to this contest");
        }
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
}
