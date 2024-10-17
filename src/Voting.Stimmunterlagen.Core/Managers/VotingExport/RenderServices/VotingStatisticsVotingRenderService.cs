// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Managers.VotingExport.Converters;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

public class VotingStatisticsVotingRenderService : IVotingRenderService
{
    private const string FileNameTemplate = "Voting_Statistics_{0}.csv";
    private readonly CsvService _csvService;
    private readonly VoterRepo _voterRepo;
    private readonly IClock _clock;

    public VotingStatisticsVotingRenderService(CsvService csvService, VoterRepo voterRepo, IClock clock, ContestDomainOfInfluenceRepo doiRepo)
    {
        _csvService = csvService;
        _voterRepo = voterRepo;
        _clock = clock;
    }

    public async Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct)
    {
        var voterStatistics = await _voterRepo.Query()
            .WhereBelongToDomainOfInfluenceOnlyVoterList(context.DomainOfInfluence.Id)
            .GroupBy(v => new { v.Sex, v.SendVotingCardsToDomainOfInfluenceReturnAddress, v.VoterType, v.VotingCardType, })
            .Select(x => new { x.Key.Sex, x.Key.SendVotingCardsToDomainOfInfluenceReturnAddress, x.Key.VoterType, x.Key.VotingCardType, Count = x.Count() })
            .ToListAsync(ct);

        var data = new Data
        {
            ExportDate = _clock.UtcNow,
            DomainOfInfluenceName = context.DomainOfInfluence.Name,
            Bfs = context.DomainOfInfluence.Bfs,
            NumberOfVoters = voterStatistics.Sum(x => x.Count),
            CountOfSendVotingCardsToDomainOfInfluenceReturnAddress = voterStatistics.Where(x => x.SendVotingCardsToDomainOfInfluenceReturnAddress).Sum(x => x.Count),
            NumberOfVotersMale = voterStatistics.Where(x => x.Sex is SexType.Male).Sum(x => x.Count),
            NumberOfVotersFemale = voterStatistics.Where(x => x.Sex is SexType.Female).Sum(x => x.Count),
            SwissAndSwissAbroadMale = voterStatistics.Where(x => x.Sex is SexType.Male && x.VoterType is VoterType.Swiss or VoterType.SwissAbroad).Sum(x => x.Count),
            SwissAndSwissAbroadFemale = voterStatistics.Where(x => x.Sex is SexType.Female && x.VoterType is VoterType.Swiss or VoterType.SwissAbroad).Sum(x => x.Count),
            ForeignerMale = voterStatistics.Where(x => x.Sex is SexType.Male && x.VoterType is VoterType.Foreigner).Sum(x => x.Count),
            ForeignerFemale = voterStatistics.Where(x => x.Sex is SexType.Female && x.VoterType is VoterType.Foreigner).Sum(x => x.Count),
            NumberOfVotersEVoting = voterStatistics.Where(x => x.VotingCardType is VotingCardType.EVoting).Sum(x => x.Count),
        };

        var csvData = await _csvService.Render(new[] { data }, ct: ct);
        return new FileModel(csvData, FileNameUtils.GenerateFileName(FileNameTemplate, new[] { context.DomainOfInfluence.Bfs }), MimeTypes.CsvMimeType);
    }

    private class Data
    {
        [TypeConverter(typeof(VotingCsvExportDateConverter))]
        [Name("Datum")]
        public DateTime ExportDate { get; set; }

        [Name("Gemeinde")]
        public string DomainOfInfluenceName { get; set; } = string.Empty;

        [Name("Bfs")]
        public string Bfs { get; set; } = string.Empty;

        [Name("Total erstellte Ausweise")]
        public int NumberOfVoters { get; set; }

        [Name("Nicht zustellen")]
        public int CountOfSendVotingCardsToDomainOfInfluenceReturnAddress { get; set; }

        [Name("Totale Ausweise für Versand")]
        public int NumberOfVotersExcludeCountOfSendVotingCardsToDomainOfInfluenceReturnAddress => NumberOfVoters - CountOfSendVotingCardsToDomainOfInfluenceReturnAddress;

        [Name("Stimmberechtigt Männer")]
        public int NumberOfVotersMale { get; set; }

        [Name("Stimmberechtigt Frauen")]
        public int NumberOfVotersFemale { get; set; }

        [Name("Anz. Personen gemäss Abstimmungsjournal")]
        public int NumberOfVotersVotingJournal => NumberOfVoters;

        [Name("Schweizer Männer")]
        public int SwissAndSwissAbroadMale { get; set; }

        [Name("Schweizer Frauen")]
        public int SwissAndSwissAbroadFemale { get; set; }

        [Name("Ausländer Männer")]
        public int ForeignerMale { get; set; }

        [Name("Ausländer Frauen")]
        public int ForeignerFemale { get; set; }

        [Name("Anzahl E-Voter")]
        public int NumberOfVotersEVoting { get; set; }
    }
}
