// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

public class EVoterByDoiOnContestRenderService : IVotingRenderService
{
    private const string FileNameTemplate = "E_Voters_By_Doi_On_Contest_{0}.csv";
    private readonly CsvService _csvService;
    private readonly DomainOfInfluenceManager _doiManager;

    public EVoterByDoiOnContestRenderService(DomainOfInfluenceManager doiManager, CsvService csvService)
    {
        _doiManager = doiManager;
        _csvService = csvService;
    }

    public async Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct)
    {
        var eVoterList = await _doiManager.ListEVoting(context.DomainOfInfluence.ContestId);
        var data = new List<Data>();
        foreach (var eVoterDoiEntry in eVoterList)
        {
            data.Add(new()
            {
                DomainOfInfluence = eVoterDoiEntry.DomainOfInfluence.Name,
                DomainOfInfluenceBfs = eVoterDoiEntry.DomainOfInfluence.Bfs,
                ParentPoliticalCount = eVoterDoiEntry.ParentPoliticalBusinessesCount,
                OwnPoliticalCount = eVoterDoiEntry.OwnPoliticalBusinessesCount,
                NumberOfVoters = eVoterDoiEntry.CountOfVotingCardsForEVoters,
            });
        }

        var csvData = await _csvService.Render(data, ct: ct);
        return new FileModel(csvData, FileNameUtils.GenerateFileName(FileNameTemplate, [context.DomainOfInfluence!.Contest!.Date.ToString("dd.MM.yyyy")]), MimeTypes.CsvMimeType);
    }

    private class Data
    {
        [Name("Bezeichnung (Gemeinde)")]
        public string DomainOfInfluence { get; set; } = string.Empty;

        [Name("BFS-Nr.")]
        public string DomainOfInfluenceBfs { get; set; } = string.Empty;

        [Name("Geschäfte auf übergeordneten Wahlkreisen")]
        public int ParentPoliticalCount { get; set; }

        [Name("Eigene Geschäfte")]
        public int OwnPoliticalCount { get; set; }

        [Name("Total betroffener E-Voter")]
        public int NumberOfVoters { get; set; }
    }
}
