// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public class VoterListBuilder
{
    private readonly DataContext _dataContext;
    private readonly IDbRepository<DataContext, DomainOfInfluenceVoterDuplicate> _voterDuplicateRepo;
    private readonly VoterListRepo _voterListRepo;

    public VoterListBuilder(
        DataContext dataContext,
        IDbRepository<DataContext, DomainOfInfluenceVoterDuplicate> voterDuplicateRepo,
        VoterListRepo voterListRepo)
    {
        _dataContext = dataContext;
        _voterDuplicateRepo = voterDuplicateRepo;
        _voterListRepo = voterListRepo;
    }

    internal async Task CleanUp(IReadOnlyCollection<Guid> contestIds)
    {
        var voterListsToDelete = await _dataContext.VoterLists
            .Where(vl => contestIds.Contains(vl.DomainOfInfluence!.ContestId) &&
                ((!vl.DomainOfInfluence!.Contest!.IsPoliticalAssembly && vl.PoliticalBusinessEntries!.Count == 0) || vl.DomainOfInfluence!.StepStates!.Count(s => s.Step == Step.VoterLists) == 0))
            .ToListAsync();

        var domainOfInfluencesToCleanUp = voterListsToDelete
            .Select(vl => vl.DomainOfInfluenceId)
            .ToHashSet();

        await _voterListRepo.DeleteRangeByKey(voterListsToDelete.ConvertAll(vl => vl.Id));
        await CleanUpDuplicatesAndUpdateVotingCardCountsForDomainOfInfluence(domainOfInfluencesToCleanUp);
    }

    internal async Task CleanUpDuplicatesAndUpdateVotingCardCountsForDomainOfInfluence(IReadOnlyCollection<Guid> domainOfInfluenceIds)
    {
        if (domainOfInfluenceIds.Count == 0)
        {
            return;
        }

        var voterDuplicates = await _dataContext.DomainOfInfluenceVoterDuplicates
            .Where(d => domainOfInfluenceIds.Contains(d.DomainOfInfluenceId))
            .Include(d => d.Voters!.OrderByDescending(v => v.List!.Index))
            .ToListAsync();

        if (voterDuplicates.Count == 0)
        {
            return;
        }

        // Ensure that at least one voter and the latest version of the voter (of the duplicate group) gets the voting card.
        var voterIdsToEnableVcPrint = voterDuplicates
            .Where(d => d.Voters!.Count > 0 && d.Voters.All(v => v.VotingCardPrintDisabled))
            .Select(d => d.Voters!.First().Id)
            .ToList();

        if (voterIdsToEnableVcPrint.Count > 0)
        {
            await _dataContext.Voters
                .Where(v => voterIdsToEnableVcPrint.Contains(v.Id))
                .ExecuteUpdateAsync(x => x.SetProperty(v => v.VotingCardPrintDisabled, _ => false));
        }

        // If a voter duplicate entry only has 1 or less voters, the duplicate entry is not needed anymore.
        var voterDuplicatesToDelete = voterDuplicates.Where(d => d.Voters!.Count() <= 1).ToList();
        if (voterDuplicatesToDelete.Count > 0)
        {
            await _voterDuplicateRepo.DeleteRangeByKey(voterDuplicatesToDelete.ConvertAll(d => d.Id));
        }

        await _voterListRepo.UpdateVotingCardCounts(domainOfInfluenceIds);
    }
}
