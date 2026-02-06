// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

public class StatisticsByReligionVotingRenderService : IVotingRenderService
{
    private const string FileNameTemplate = "Statistics_By_Religion_{0}.csv";
    private readonly CsvService _csvService;
    private readonly VoterRepo _voterRepo;

    public StatisticsByReligionVotingRenderService(CsvService csvService, VoterRepo voterRepo)
    {
        _csvService = csvService;
        _voterRepo = voterRepo;
    }

    public async Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct)
    {
        var voterStatistics = await _voterRepo.Query()
            .OrderBy(x => x.Religion)
            .WhereBelongToDomainOfInfluenceOnlyVoterList(context.DomainOfInfluence.Id)
            .WhereVotingCardPrintEnabled()
            .Where(v => context.VoterList == null || v.ListId == context.VoterList.Id)
            .GroupBy(v => new { v.Religion, v.Sex, v.SendVotingCardsToDomainOfInfluenceReturnAddress })
            .Select(x => new { x.Key.Religion, x.Key.Sex, x.Key.SendVotingCardsToDomainOfInfluenceReturnAddress, Count = x.Count() })
            .ToListAsync(ct);

        var religionCodesByReligiousDenomination = voterStatistics.Select(x => x.Religion).ToHashSet()
            .GroupBy(ReligionMapping.GetReligiousDenomination)
            .ToImmutableSortedDictionary(x => x.Key, x => x.ToList());

        var data = new List<Data>();

        foreach (var (religiousDenomination, religionCodes) in religionCodesByReligiousDenomination)
        {
            var religionVoterStatistics = voterStatistics.Where(x => religionCodes.Contains(x.Religion)).ToList();

            data.Add(new()
            {
                ReligiousDenomination = religiousDenomination,
                NumberOfVotersMale = religionVoterStatistics.Where(x => x.Sex is SexType.Male).Sum(x => x.Count),
                NumberOfVotersFemale = religionVoterStatistics.Where(x => x.Sex is SexType.Female).Sum(x => x.Count),
                CountOfSendVotingCardsToDomainOfInfluenceReturnAddressMale = religionVoterStatistics.Where(x => x.Sex is SexType.Male && x.SendVotingCardsToDomainOfInfluenceReturnAddress).Sum(x => x.Count),
                CountOfSendVotingCardsToDomainOfInfluenceReturnAddressFemale = religionVoterStatistics.Where(x => x.Sex is SexType.Female && x.SendVotingCardsToDomainOfInfluenceReturnAddress).Sum(x => x.Count),
            });
        }

        var csvData = await _csvService.Render(data, ct: ct);
        return new FileModel(csvData, FileNameUtils.GenerateFileName(FileNameTemplate, context.BuildVotingJournalFileNameArgs()), MimeTypes.CsvMimeType);
    }

    private class Data
    {
        [Name("Konfession")]
        public string ReligiousDenomination { get; set; } = string.Empty;

        [Name("Total männl.")]
        public int NumberOfVotersMale { get; set; }

        [Name("Total weibl.")]
        public int NumberOfVotersFemale { get; set; }

        [Name("nicht zustellen männl.")]
        public int CountOfSendVotingCardsToDomainOfInfluenceReturnAddressMale { get; set; }

        [Name("nicht zustellen weibl.")]
        public int CountOfSendVotingCardsToDomainOfInfluenceReturnAddressFemale { get; set; }
    }
}
