// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class ApproveContestStepManager : ISingleStepManager
{
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IClock _clock;

    public ApproveContestStepManager(IDbRepository<Contest> contestRepo, IClock clock)
    {
        _contestRepo = contestRepo;
        _clock = clock;
    }

    public Step Step => Step.ContestApproval;

    public async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var contest = await _contestRepo.Query()
                          .WhereIsContestManager(tenantId)
                          .FirstOrDefaultAsync(c => c.DomainOfInfluenceId == domainOfInfluenceId, ct)
                      ?? throw new EntityNotFoundException(nameof(Contest), $"{domainOfInfluenceId}-{tenantId}");

        if (!contest.PrintingCenterSignUpDeadline.HasValue
            || !contest.AttachmentDeliveryDeadline.HasValue
            || !contest.GenerateVotingCardsDeadline.HasValue)
        {
            throw new ValidationException("deadlines need to be specified to approve the contest");
        }

        contest.Approved = _clock.UtcNow;
        await _contestRepo.Update(contest);
    }

    public async Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var contest = await _contestRepo.Query()
                          .WhereIsContestManager(tenantId)
                          .FirstOrDefaultAsync(c => c.DomainOfInfluenceId == domainOfInfluenceId, ct)
                      ?? throw new EntityNotFoundException(nameof(Contest), $"{domainOfInfluenceId}-{tenantId}");

        contest.Approved = null;
        await _contestRepo.Update(contest);
    }
}
