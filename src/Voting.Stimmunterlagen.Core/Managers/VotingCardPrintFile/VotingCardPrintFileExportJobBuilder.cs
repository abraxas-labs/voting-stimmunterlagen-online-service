// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class VotingCardPrintFileExportJobBuilder
{
    private const string FileExtension = ".csv";

    private readonly IDbRepository<VotingCardPrintFileExportJob> _votingCardPrintFileExportJobRepo;
    private readonly IDbRepository<VotingCardGeneratorJob> _votingCardGeneratorJobRepo;

    public VotingCardPrintFileExportJobBuilder(
        IDbRepository<VotingCardPrintFileExportJob> votingCardPrintFileExportJobRepo,
        IDbRepository<VotingCardGeneratorJob> votingCardGeneratorJobRepo)
    {
        _votingCardPrintFileExportJobRepo = votingCardPrintFileExportJobRepo;
        _votingCardGeneratorJobRepo = votingCardGeneratorJobRepo;
    }

    internal async Task<List<VotingCardPrintFileExportJob>> CleanAndBuildJobs(Guid doiId)
    {
        await CleanJobs(doiId);

        var votingCardGeneratorJobs = await _votingCardGeneratorJobRepo.Query()
            .WhereHasDomainOfInfluence(doiId)
            .ToListAsync();

        if (votingCardGeneratorJobs.Any(x => x.Completed == null && x.State != VotingCardGeneratorJobState.ReadyToRunOffline))
        {
            throw new ValidationException("Cannot build the print file export jobs when any voting card generator job is not completed which needs to run online");
        }

        var votingCardPrintFileExportJobs = votingCardGeneratorJobs.ConvertAll(BuildJob);
        await _votingCardPrintFileExportJobRepo.CreateRange(votingCardPrintFileExportJobs);
        return votingCardPrintFileExportJobs;
    }

    private VotingCardPrintFileExportJob BuildJob(
        VotingCardGeneratorJob votingCardGeneratorJob)
    {
        return new VotingCardPrintFileExportJob
        {
            State = ExportJobState.ReadyToRun,
            FileName = BuildFileName(votingCardGeneratorJob),
            VotingCardGeneratorJobId = votingCardGeneratorJob.Id,
        };
    }

    private string BuildFileName(VotingCardGeneratorJob votingCardGeneratorJob)
    {
        return Path.GetFileNameWithoutExtension(votingCardGeneratorJob.FileName) + FileExtension;
    }

    private async Task CleanJobs(Guid doiId)
    {
        var toDelete = await _votingCardPrintFileExportJobRepo.Query()
            .WhereHasDomainOfInfluence(doiId)
            .Select(x => x.Id)
            .ToListAsync();

        await _votingCardPrintFileExportJobRepo.DeleteRangeByKey(toDelete);
    }
}
