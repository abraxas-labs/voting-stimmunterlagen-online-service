// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class StepsBuilder
{
    private readonly IDbRepository<Contest> _contestRepo;
    private readonly IDbRepository<StepState> _stepRepo;
    private readonly AttachmentBuilder _attachmentBuilder;
    private readonly VoterListBuilder _voterListBuilder;

    public StepsBuilder(
        IDbRepository<Contest> contestRepo,
        IDbRepository<StepState> stepRepo,
        AttachmentBuilder attachmentBuilder,
        VoterListBuilder voterListBuilder)
    {
        _contestRepo = contestRepo;
        _stepRepo = stepRepo;
        _attachmentBuilder = attachmentBuilder;
        _voterListBuilder = voterListBuilder;
    }

    public Task SyncStepsForBasisDomainOfInfluence(Guid basisDoiId)
        => SyncSteps(query => query.Where(x => x.BasisDomainOfInfluenceId == basisDoiId));

    public Task SyncStepsForContest(Guid contestId)
        => SyncSteps(query => query.Where(x => x.ContestId == contestId));

    private async Task SyncSteps(Func<IQueryable<ContestDomainOfInfluence>, IQueryable<ContestDomainOfInfluence>>? queryAdjustor = null)
    {
        var query = _contestRepo.Query()
            .WhereInTestingPhase()
            .SelectMany(x => x.ContestDomainOfInfluences!)
            .Include(x => x.ParentHierarchyEntries!).ThenInclude(y => y.DomainOfInfluence);
        var items = await (queryAdjustor?.Invoke(query) ?? query)
            .Select(doi => new
            {
                DomainOfInfluenceId = doi.Id,
                Steps = doi.StepStates,
                ContestRole = doi.Role,
                PoliticalBusinessRoles = doi.PoliticalBusinessPermissionEntries!
                    .Select(x => x.Role)
                    .ToList(),
                IsSingleAttendeeContest = doi.Contest!.IsSingleAttendeeContest || (doi.Contest.IsPoliticalAssembly && !doi.ParentHierarchyEntries!
                    .Where(x => x.DomainOfInfluence!.ResponsibleForVotingCards)
                    .Select(x => x.DomainOfInfluenceId)
                    .Distinct()
                    .Any()),
                doi.Contest.EVoting,
                doi.ExternalPrintingCenter,
                doi.Contest.IsPoliticalAssembly,
                doi.ResponsibleForVotingCards,
                doi.ContestId,
            })
            .ToListAsync();

        var toRemove = new List<StepState>();
        var toAdd = new List<StepState>();

        foreach (var item in items)
        {
            var diff = StepsDiffBuilder.BuildStepsDiff(
                item.DomainOfInfluenceId,
                item.Steps,
                item.ContestRole,
                item.PoliticalBusinessRoles,
                item.IsSingleAttendeeContest,
                item.EVoting,
                item.ExternalPrintingCenter,
                item.IsPoliticalAssembly,
                item.ResponsibleForVotingCards);
            toRemove.AddRange(diff.ToRemove);
            toAdd.AddRange(diff.ToAdd);
        }

        await _stepRepo.DeleteRangeByKey(toRemove.Select(x => x.Id));
        await _stepRepo.CreateRange(toAdd);
        await CleanUpForRemovedSteps(toRemove.ConvertAll(s => s.Step), items.Select(i => i.ContestId).Distinct().ToList());
    }

    private async Task CleanUpForRemovedSteps(IReadOnlyCollection<Step> removedSteps, List<Guid> contestIds)
    {
        if (removedSteps.Contains(Step.Attachments))
        {
            await _attachmentBuilder.CleanUp(contestIds);
        }

        if (removedSteps.Contains(Step.VoterLists))
        {
            await _voterListBuilder.CleanUp(contestIds);
        }
    }
}
