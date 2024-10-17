// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class ApprovePoliticalBusinessesStepManager : ISingleStepManager
{
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;

    public ApprovePoliticalBusinessesStepManager(
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<PoliticalBusiness> politicalBusinessRepo)
    {
        _doiRepo = doiRepo;
        _politicalBusinessRepo = politicalBusinessRepo;
    }

    public Step Step => Step.PoliticalBusinessesApproval;

    public Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
        => SetApproved(domainOfInfluenceId, tenantId, true, ct);

    public Task Revert(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
        => SetApproved(domainOfInfluenceId, tenantId, false, ct);

    private async Task SetApproved(Guid domainOfInfluenceId, string tenantId, bool approved, CancellationToken ct)
    {
        // according to business this step should approve all businesses of the current tenant,
        // regardless of the domain of influence.
        var dois = await _doiRepo
            .Query()
            .WhereIsManager(tenantId)
            .Include(x => x.PoliticalBusinesses!.Where(y => y.Approved != approved))
            .Include(x => x.PrintJob)
            .ToListAsync(ct);

        var businesses = dois.SelectMany(x => x.PoliticalBusinesses!).ToList();

        foreach (var business in businesses)
        {
            business.Approved = approved;
        }

        await _politicalBusinessRepo.UpdateRange(businesses);
        SetPrintJobState(dois, approved);

        await _doiRepo.UpdateRange(dois);
    }

    private void SetPrintJobState(IEnumerable<ContestDomainOfInfluence> dois, bool approved)
    {
        foreach (var doi in dois)
        {
            if (doi.PrintJob == null || doi.PrintJob.AllAttachmentsOnceDelivered())
            {
                continue;
            }

            doi.PrintJob.State = approved
                ? PrintJobState.SubmissionOngoing
                : PrintJobState.Empty;
        }
    }
}
