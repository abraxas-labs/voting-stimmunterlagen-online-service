// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class StepManager
{
    private static readonly HashSet<Step> ImmutableSteps = new()
    {
        // this step is only an overview, it gets approved/reverted by the printing center (DVZ)
        Step.PrintJobOverview,
        Step.GenerateManualVotingCards,
        Step.ContestOverview,
        Step.VotingJournal,
    };

    private readonly IDbRepository<StepState> _stepStateRepo;
    private readonly IAuth _auth;
    private readonly IReadOnlyDictionary<Step, ISingleStepManager> _singleStepManagers;
    private readonly StepsBuilder _stepsBuilder;
    private readonly IClock _clock;
    private readonly DataContext _dbContext;

    public StepManager(
        IAuth auth,
        IDbRepository<StepState> stepStateRepo,
        IEnumerable<ISingleStepManager> singleStepManagers,
        StepsBuilder stepsBuilder,
        IClock clock,
        DataContext dbContext)
    {
        _stepStateRepo = stepStateRepo;
        _stepsBuilder = stepsBuilder;
        _auth = auth;
        _clock = clock;
        _dbContext = dbContext;

        // this activates all single step managers, shouldn't be too much of a perf issue for now
        // if it gets a perf issue later, switch to a lazy approach
        _singleStepManagers = singleStepManagers.ToDictionary(x => x.Step);
    }

    public async Task<List<StepState>> List(Guid domainOfInfluenceId)
    {
        var tenantId = _auth.Tenant.Id;
        return await _stepStateRepo.Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .Where(x => x.DomainOfInfluenceId == domainOfInfluenceId)
            .OrderBy(x => x.Step)
            .ToListAsync();
    }

    public async Task EnsureStepApproved(Guid domainOfInfluenceId, Step step, bool approved = true)
    {
        if (!await _stepStateRepo.Query()
            .AnyAsync(x => x.Approved == approved && x.Step == step && x.DomainOfInfluenceId == domainOfInfluenceId))
        {
            throw new ValidationException(
                $"{step} not found or has not the correct state ({nameof(approved)}: {approved}, {nameof(domainOfInfluenceId)}{domainOfInfluenceId})");
        }
    }

    public Task Approve(Guid domainOfInfluenceId, Step step, CancellationToken ct)
        => SetStepApproved(domainOfInfluenceId, step, true, ct);

    public Task Revert(Guid domainOfInfluenceId, Step step, CancellationToken ct)
        => SetStepApproved(domainOfInfluenceId, step, false, ct);

    private async Task SetStepApproved(Guid domainOfInfluenceId, Step step, bool approved, CancellationToken ct)
    {
        if (ImmutableSteps.Contains(step))
        {
            throw new ValidationException($"the step {step} is not directly mutable");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var tenantId = _auth.Tenant.Id;
        var stepStates = await _stepStateRepo.Query()
            .WhereContestIsNotLocked()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .Where(x => x.DomainOfInfluenceId == domainOfInfluenceId)
            .Include(x => x.DomainOfInfluence!.Contest)
            .OrderBy(x => x.Step)
            .ToListAsync(ct);
        var stepState = stepStates.Find(x => x.Step == step)
                        ?? throw new EntityNotFoundException(nameof(StepState), $"{domainOfInfluenceId}-{step}");

        EnsureIsNotPastContestDeadlines(stepState);

        if (stepState.Approved == approved)
        {
            throw new ValidationException("step has a matching approved state already");
        }

        // approve: check all steps before this step are approved (only the first not approved step can be approved)
        if (approved && stepStates.Any(s => s.Step < step && !s.Approved))
        {
            throw new ValidationException($"not all steps before {step} are approved");
        }

        // revert: check all steps after this step are not approved (only the last approved step can be reverted)
        if (!approved && stepStates.Any(s => s.Step > step && s.Approved))
        {
            throw new ValidationException("only the last approved step can be reverted");
        }

        stepState.Approved = approved;
        stepState.DomainOfInfluence = null;
        await _stepStateRepo.Update(stepState);

        var manager = GetStepManager(step);
        if (manager != null)
        {
            if (approved)
            {
                await manager.Approve(domainOfInfluenceId, tenantId, ct);
            }
            else
            {
                await manager.Revert(domainOfInfluenceId, tenantId, ct);
            }
        }

        await transaction.CommitAsync();
    }

    private ISingleStepManager? GetStepManager(Step step)
    {
        _singleStepManagers.TryGetValue(step, out var manager);
        return manager;
    }

    private void EnsureIsNotPastContestDeadlines(StepState stepState)
    {
        var step = stepState.Step;
        var contest = stepState.DomainOfInfluence!.Contest!;

        if (step >= Step.PrintJobOverview || step == Step.EVoting || step == Step.ContestApproval)
        {
            return;
        }

        if ((step <= Step.Attachments) && !(contest.PrintingCenterSignUpDeadline == null || contest.PrintingCenterSignUpDeadline.Value > _clock.UtcNow))
        {
            throw new ValidationException($"Step {step} on contest {contest.Id} is not approvable because it is past printing center sign up deadline");
        }

        if (step >= Step.VoterLists && !(contest.GenerateVotingCardsDeadline != null && contest.GenerateVotingCardsDeadline > _clock.UtcNow))
        {
            throw new ValidationException($"Step {step} on contest {contest.Id} is not approvable because it is past generate voting cards deadline");
        }
    }
}
