// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class ContestBuilder
{
    private readonly ContestRepo _contestRepo;
    private readonly IDbRepository<ContestTranslation> _contestTranslationRepo;
    private readonly IMapper _mapper;
    private readonly ContestDomainOfInfluenceBuilder _contestDomainOfInfluenceBuilder;
    private readonly ContestCountingCircleBuilder _contestCountingCircleBuilder;
    private readonly StepsBuilder _stepsBuilder;
    private readonly DomainOfInfluenceVotingCardLayoutBuilder _doiVotingCardLayoutBuilder;
    private readonly ContestVotingCardLayoutBuilder _contestVotingCardLayoutBuilder;
    private readonly DomainOfInfluenceVotingCardConfigurationBuilder _doiVotingCardConfigurationBuilder;
    private readonly PrintJobBuilder _printJobBuilder;
    private readonly ContestEVotingExportJobBuilder _eVotingExportJobBuilder;
    private readonly ContestOrderNumberStateBuilder _contestOrderNumberStateBuilder;

    public ContestBuilder(
        ContestRepo contestRepo,
        IDbRepository<ContestTranslation> contestTranslationRepo,
        IMapper mapper,
        ContestDomainOfInfluenceBuilder contestDomainOfInfluenceBuilder,
        ContestCountingCircleBuilder contestCountingCircleBuilder,
        StepsBuilder stepsBuilder,
        DomainOfInfluenceVotingCardLayoutBuilder doiVotingCardLayoutBuilder,
        ContestVotingCardLayoutBuilder contestVotingCardLayoutBuilder,
        DomainOfInfluenceVotingCardConfigurationBuilder doiVotingCardConfigurationBuilder,
        PrintJobBuilder printJobBuilder,
        ContestEVotingExportJobBuilder eVotingExportJobBuilder,
        ContestOrderNumberStateBuilder contestOrderNumberStateBuilder)
    {
        _contestRepo = contestRepo;
        _contestTranslationRepo = contestTranslationRepo;
        _mapper = mapper;
        _contestDomainOfInfluenceBuilder = contestDomainOfInfluenceBuilder;
        _contestCountingCircleBuilder = contestCountingCircleBuilder;
        _stepsBuilder = stepsBuilder;
        _doiVotingCardLayoutBuilder = doiVotingCardLayoutBuilder;
        _contestVotingCardLayoutBuilder = contestVotingCardLayoutBuilder;
        _doiVotingCardConfigurationBuilder = doiVotingCardConfigurationBuilder;
        _printJobBuilder = printJobBuilder;
        _eVotingExportJobBuilder = eVotingExportJobBuilder;
        _contestOrderNumberStateBuilder = contestOrderNumberStateBuilder;
    }

    internal async Task CreateOrUpdateContest(ContestEventData contestData)
    {
        var id = GuidParser.Parse(contestData.Id);
        var existingContest = await GetExistingContest(id);

        if (existingContest == null)
        {
            await CreateContest(_mapper.Map<Contest>(contestData));
            return;
        }

        // the domain of influence is immutable for a contest
        var doiId = existingContest.DomainOfInfluenceId;
        _mapper.Map(contestData, existingContest);
        existingContest.DomainOfInfluenceId = doiId;

        await UpdateExistingContest(existingContest);
    }

    internal async Task CreateOrUpdatePoliticalAssembly(PoliticalAssemblyEventData politicalAssemblyData)
    {
        var id = GuidParser.Parse(politicalAssemblyData.Id);
        var existingContest = await GetExistingContest(id);

        if (existingContest == null)
        {
            var contest = _mapper.Map<Contest>(politicalAssemblyData);
            contest.IsPoliticalAssembly = true;
            await CreateContest(contest);
            return;
        }

        // the domain of influence is immutable for a contest
        var doiId = existingContest.DomainOfInfluenceId;
        _mapper.Map(politicalAssemblyData, existingContest);
        existingContest.DomainOfInfluenceId = doiId;

        await UpdateExistingContest(existingContest);
    }

    internal async Task CreateContest(Contest contest)
    {
        var contestManagerDoiId = contest.DomainOfInfluenceId;
        contest.DomainOfInfluenceId = null;
        contest.ContestDomainOfInfluences = new List<ContestDomainOfInfluence>();
        contest.ContestCountingCircles = new List<ContestCountingCircle>();
        contest.OrderNumber = await _contestOrderNumberStateBuilder.NextOrderNumber(contest.Date);
        await _contestRepo.Create(contest);

        // if needed, this is updated when the contest domain of influences are created.
        contest.DomainOfInfluenceId = contestManagerDoiId;

        await _contestDomainOfInfluenceBuilder.CreateMissingDataForContest(contest);
        await _contestCountingCircleBuilder.CreateMissingDataForContest(contest);
        await SyncContestRelatedData(contest.Id);

        await _contestRepo.CreateVoterContestIndexSequence(contest.Id);
    }

    [Obsolete("contest counting circle options are deprecated")]
    internal async Task UpdateContestCountingCircleOptions(string contestId, IEnumerable<ContestCountingCircleOptionEventData> contestCountingCircleOptions)
    {
        var id = GuidParser.Parse(contestId);
        var contest = await _contestRepo.Query()
            .Include(x => x.ContestDomainOfInfluences!).ThenInclude(x => x.HierarchyEntries)
            .Include(x => x.ContestCountingCircles)
            .FirstAsync(x => x.Id == id);

        var optionsById = contestCountingCircleOptions
            .GroupBy(x => x.CountingCircleId)
            .ToDictionary(x => x.Key, x => x.Single());

        foreach (var countingCircle in contest.ContestCountingCircles!)
        {
            if (optionsById.TryGetValue(countingCircle.BasisCountingCircleId.ToString(), out var contestCountingCircleOption))
            {
                countingCircle.EVoting = contestCountingCircleOption.EVoting;
            }
        }

        await _contestRepo.Update(contest);
    }

    private async Task SyncContestRelatedData(Guid contestId)
    {
        await _stepsBuilder.SyncStepsForContest(contestId);
        await _contestVotingCardLayoutBuilder.Sync(contestId);
        await _doiVotingCardLayoutBuilder.SyncForContest(contestId);
        await _doiVotingCardConfigurationBuilder.SyncForContest(contestId);
        await _printJobBuilder.SyncForContest(contestId);
        await _eVotingExportJobBuilder.SyncForContest(contestId);
    }

    private async Task DeleteContestTranslations(Guid contestId)
    {
        var translationIds = await _contestTranslationRepo.Query()
            .IgnoreQueryFilters() // do not filter translations
            .Where(x => x.ContestId == contestId)
            .Select(x => x.Id)
            .ToListAsync();
        await _contestTranslationRepo.DeleteRangeByKey(translationIds);
    }

    private async Task<Contest?> GetExistingContest(Guid contestId)
    {
        return await _contestRepo.Query()
            .Include(x => x.ContestDomainOfInfluences!).ThenInclude(x => x.HierarchyEntries)
            .Include(x => x.ContestCountingCircles)
            .FirstOrDefaultAsync(x => x.Id == contestId);
    }

    private async Task UpdateExistingContest(Contest existingContest)
    {
        await DeleteContestTranslations(existingContest.Id);
        await _contestRepo.Update(existingContest);

        await _contestDomainOfInfluenceBuilder.CreateMissingDataForContest(existingContest);
        await _contestCountingCircleBuilder.CreateMissingDataForContest(existingContest);
        await SyncContestRelatedData(existingContest.Id);
    }
}
