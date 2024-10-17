// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class ContestEVotingExportJobBuilder
{
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<ContestEVotingExportJob> _jobsRepo;

    public ContestEVotingExportJobBuilder(
        IDbRepository<Contest> contestRepo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<ContestEVotingExportJob> jobsRepo)
    {
        _contestRepo = contestRepo;
        _doiRepo = doiRepo;
        _jobsRepo = jobsRepo;
    }

    internal async Task SyncForContest(Guid contestId)
    {
        var contest = await _contestRepo.Query()
            .Where(c => c.Id == contestId)
            .Include(c => c.DomainOfInfluence)
            .Include(c => c.EVotingExportJob)
            .Include(c => c.Translations)
            .FirstOrDefaultAsync() ?? throw new EntityNotFoundException(nameof(Contest), contestId);

        if (!contest.EVoting)
        {
            if (contest.EVotingExportJob != null)
            {
                await _jobsRepo.DeleteByKey(contest.EVotingExportJob.Id);
            }

            return;
        }

        if (contest.EVotingExportJob != null)
        {
            var exportJob = contest.EVotingExportJob;
            if (exportJob.State <= ExportJobState.Pending)
            {
                exportJob.FileName = BuildFileName(contest);
                await _jobsRepo.UpdateIgnoreRelations(exportJob);
            }

            return;
        }

        await CreateNewExportJob(contest);
    }

    internal async Task<Guid> PrepareJobToRun(Guid doiId, string tenantId, CancellationToken ct)
    {
        var doi = await _doiRepo.Query()
            .WhereIsContestManager(tenantId)
            .Include(x => x.Contest!.DomainOfInfluence)
            .Include(x => x.Contest!.Translations)
            .FirstOrDefaultAsync(doi => doi.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);

        var existingExportJob = await _jobsRepo.Query()
            .Where(x => x.ContestId == doi.ContestId)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException(nameof(ContestEVotingExportJob), doi.ContestId);

        await _jobsRepo.DeleteByKey(existingExportJob.Id);
        var newExportJob = new ContestEVotingExportJob
        {
            ContestId = existingExportJob.ContestId,
            FileName = BuildFileName(doi.Contest!),
            State = ExportJobState.ReadyToRun,
        };
        await _jobsRepo.Create(newExportJob);
        return newExportJob.Id;
    }

    private async Task<Guid> CreateNewExportJob(Contest contest)
    {
        var newJob = new ContestEVotingExportJob
        {
            ContestId = contest.Id,
            FileName = BuildFileName(contest),
            State = ExportJobState.Pending,
        };

        await _jobsRepo.Create(newJob);
        return newJob.Id;
    }

    private string BuildFileName(Contest contest)
    {
        var canton = contest.DomainOfInfluence!.Canton.ToString().ToUpper();
        var description = contest.Translations!.FirstOrDefault(t => t.Language.Equals(Languages.German))?.Description;

        return $"ech0045v4_{canton}_{contest.Date:yyyyMMdd}_{description}_EVoting.zip";
    }
}
