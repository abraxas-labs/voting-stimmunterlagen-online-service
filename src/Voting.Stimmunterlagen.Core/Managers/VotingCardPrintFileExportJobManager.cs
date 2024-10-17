// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VotingCardPrintFileExportJobManager
{
    private readonly IDbRepository<VotingCardPrintFileExportJob> _jobsRepo;
    private readonly VotingCardPrintFileExportJobLauncher _launcher;
    private readonly IAuth _auth;
    private readonly DataContext _dbContext;

    public VotingCardPrintFileExportJobManager(
        IDbRepository<VotingCardPrintFileExportJob> jobsRepo,
        VotingCardPrintFileExportJobLauncher launcher,
        IAuth auth,
        DataContext dbContext)
    {
        _jobsRepo = jobsRepo;
        _launcher = launcher;
        _auth = auth;
        _dbContext = dbContext;
    }

    public async Task<List<VotingCardPrintFileExportJob>> ListJobs(Guid doiId, bool forPrintJobManagement)
    {
        var query = _jobsRepo.Query()
            .WhereHasDomainOfInfluence(doiId);

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        return await query
            .OrderBy(x => x.State)
            .ThenBy(x => x.FileName)
            .ToListAsync();
    }

    public async Task RetryJobs(Guid doiId)
    {
        List<VotingCardPrintFileExportJob> jobs;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        jobs = await _jobsRepo.Query()
            .WhereHasDomainOfInfluence(doiId)
            .WhereInState(ExportJobState.ReadyToRun, ExportJobState.Failed)
            .WhereContestInTestingPhase()
            .WhereDomainOfInfluenceIsNotExternalPrintingCenter()
            .ToListAsync();

        foreach (var job in jobs)
        {
            job.State = ExportJobState.ReadyToRun;
        }

        await _jobsRepo.UpdateRange(jobs);
        await transaction.CommitAsync();

        _ = _launcher.RunJobs(jobs.Select(x => x.Id));
    }
}
