// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Managers.Stimmregister;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Stistat;

public class StistatExportManager
{
    /// <summary>
    /// The stimmregister writes this value for EVoting=true.
    /// </summary>
    private const string EVotingYesValue = "Ja";

    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        HasHeaderRecord = true,
        TrimOptions = TrimOptions.Trim,
    };

    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly StimmregisterElectoralRegisterClient _stimmregisterClient;
    private readonly IStistatFileStore _stistatFileStore;
    private readonly ILogger<StistatExportManager> _logger;
    private readonly IMapper _mapper;

    public StistatExportManager(
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IDbRepository<Contest> contestRepo,
        StimmregisterElectoralRegisterClient stimmregisterClient,
        IStistatFileStore stistatFileStore,
        ILogger<StistatExportManager> logger,
        IMapper mapper)
    {
        _doiRepo = doiRepo;
        _contestRepo = contestRepo;
        _stimmregisterClient = stimmregisterClient;
        _stistatFileStore = stistatFileStore;
        _logger = logger;
        _mapper = mapper;
    }

    internal async Task RunExportIfNeeded(ContestDomainOfInfluence doi, CancellationToken ct)
    {
        if (!doi.StistatMunicipality)
        {
            return;
        }

        var exportId = Guid.NewGuid();
        using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["ExportId"] = exportId });

        var contestId = doi.ContestId;

        var contestDoi = await _contestRepo.Query()
            .Where(x => x.Id == contestId)
            .Select(x => x.DomainOfInfluence!)
            .SingleAsync(ct);
        if (!contestDoi.StistatExportEnabled)
        {
            _logger.LogInformation("Stistat export is not enabled for contest {ContestId}, skipping", contestId);
            return;
        }

        var allImported = await _doiRepo.Query()
            .Where(x => x.ContestId == contestId && x.StistatMunicipality)
            .AllAsync(x => x.VoterListImports!.Count > 0, ct);
        if (!allImported)
        {
            _logger.LogInformation("Not all stistat municipalities have voter lists imported yet, skipping");
            return;
        }

        _logger.LogInformation("All stistat municipalities have voter lists imported, starting export for contest {ContestId}", contestId);
        var filterVersionIds = await _doiRepo.Query()
            .Where(x => x.ContestId == contestId && x.StistatMunicipality)
            .SelectMany(x => x.VoterListImports!.Where(i => i.Source == VoterListSource.VotingStimmregisterFilterVersion).Select(i => Guid.Parse(i.SourceId)))
            .ToListAsync(ct);

        var pipe = new Pipe();
        var writeTask = BuildFilteredCsv(pipe.Writer, filterVersionIds, ct);

        try
        {
            await using var readStream = pipe.Reader.AsStream();
            var fileName = $"stistat_export_{contestId}.csv";
            await _stistatFileStore.Save(fileName, readStream, contestDoi.StistatExportEaiMessageType, ct);
        }
        finally
        {
            await writeTask;
        }

        _logger.LogInformation("Stistat export completed for contest {ContestId}", contestId);
    }

    private async Task BuildFilteredCsv(PipeWriter pipeWriter, List<Guid> filterVersionIds, CancellationToken ct)
    {
        try
        {
            await using var stream = pipeWriter.AsStream();
            await using var writer = new StreamWriter(stream, Encoding.UTF8);
            await using var csvWriter = new CsvWriter(writer, CsvConfig);

            csvWriter.WriteHeader<PersonStistatCsvSimplifiedExportModel>();
            await csvWriter.NextRecordAsync();

            foreach (var filterVersionId in filterVersionIds)
            {
                await AppendFilteredCsv(filterVersionId, csvWriter, ct);
            }
        }
        catch (Exception ex)
        {
            await pipeWriter.CompleteAsync(ex);
            throw;
        }

        await pipeWriter.CompleteAsync();
    }

    private async Task AppendFilteredCsv(Guid filterVersionId, CsvWriter csvWriter, CancellationToken ct)
    {
        _logger.LogInformation("Exporting {FilterVersionId}", filterVersionId);

        await using var filterVersionImportStream = await _stimmregisterClient.StreamStistat(filterVersionId, ct);
        using var reader = new StreamReader(filterVersionImportStream);
        using var csvReader = new CsvReader(reader, CsvConfig);

        await foreach (var record in csvReader.GetRecordsAsync<PersonStistatCsvImportModel>(ct))
        {
            // only export records with EVoting enabled
            if (record.EVoting != EVotingYesValue)
            {
                continue;
            }

            var exportRecord = _mapper.Map<PersonStistatCsvSimplifiedExportModel>(record);
            csvWriter.WriteRecord(exportRecord);
            await csvWriter.NextRecordAsync();
        }

        _logger.LogInformation("Exporting {FilterVersionId} done", filterVersionId);
    }
}
