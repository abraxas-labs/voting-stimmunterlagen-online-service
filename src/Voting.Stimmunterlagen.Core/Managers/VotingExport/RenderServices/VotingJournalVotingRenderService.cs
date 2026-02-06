// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Extensions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

public class VotingJournalVotingRenderService : IVotingRenderService
{
    private const string FileNameTemplate = "Voting_Journal_{0}.csv";

    private readonly VoterRepo _voterRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardConfiguration> _doiConfigRepo;
    private readonly CsvService _csvService;

    public VotingJournalVotingRenderService(
        VoterRepo voterRepo,
        CsvService csvService,
        IDbRepository<DomainOfInfluenceVotingCardConfiguration> doiConfigRepo)
    {
        _voterRepo = voterRepo;
        _csvService = csvService;
        _doiConfigRepo = doiConfigRepo;
    }

    public async Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct)
    {
        var vcConfig = await _doiConfigRepo.Query()
            .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == context.DomainOfInfluence.Id, ct)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardConfiguration), context.DomainOfInfluence.Id);

        var data = await _voterRepo.Query()
            .WhereBelongToDomainOfInfluenceOnlyVoterList(context.DomainOfInfluence.Id)
            .WhereVotingCardPrintEnabled()
            .Where(v => context.VoterList == null || v.ListId == context.VoterList.Id)
            .ToAsyncEnumerable()
            .OrderBySortingCriteriaAsync(vcConfig.Sorts)
            .Select(v => new Data
            {
                LastName = v.LastName,
                FirstName = v.FirstName,
                Address = $"{v.Street} {v.HouseNumber} {v.ZipCode} {v.Town}",
                PersonId = DatamatrixMapping.MapPersonId(v.PersonId),
                ReligiousDenomination = ReligionMapping.GetReligiousDenomination(v.Religion),
                DateOfBirth = FormatDateOfBirth(v.DateOfBirth),
            })
            .ToListAsync(ct);

        var csvData = await _csvService.Render(data, ct: ct);
        return new FileModel(csvData, FileNameUtils.GenerateFileName(FileNameTemplate, context.BuildVotingJournalFileNameArgs()), MimeTypes.CsvMimeType);
    }

    private static string FormatDateOfBirth(string dateOfBirth)
    {
        return string.Join('.', dateOfBirth.Split('-').Reverse());
    }

    private class Data
    {
        [Name("Name")]
        public string LastName { get; set; } = string.Empty;

        [Name("Vorname")]
        public string FirstName { get; set; } = string.Empty;

        [Name("Adresse")]
        public string Address { get; set; } = string.Empty;

        [Name("Personen-Nr")]
        public string PersonId { get; set; } = string.Empty;

        [Name("Geb.Dat.")]
        public string DateOfBirth { get; set; } = string.Empty;

        [Name("Konf")]
        public string ReligiousDenomination { get; set; } = string.Empty;
    }
}
