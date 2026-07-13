// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Steps;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VoterListManager
{
    private readonly IDbRepository<VoterList> _repo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IDbRepository<DomainOfInfluenceVoterDuplicate> _doiVoterDuplicateRepo;
    private readonly PoliticalBusinessVoterListEntryRepo _politicalBusinessVoterListEntryRepo;
    private readonly IAuth _auth;
    private readonly AttachmentManager _attachmentManager;
    private readonly IClock _clock;
    private readonly IDbRepository<VoterListImport> _voterListImportRepo;
    private readonly VoterRepo _voterRepo;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _dbContext;
    private readonly StepManager _stepManager;

    public VoterListManager(
        IDbRepository<VoterList> repo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        PoliticalBusinessVoterListEntryRepo politicalBusinessVoterListEntryRepo,
        IDbRepository<DomainOfInfluenceVoterDuplicate> doiVoterDuplicateRepo,
        IAuth auth,
        AttachmentManager attachmentManager,
        IClock clock,
        IDbRepository<VoterListImport> voterListImportRepo,
        VoterRepo voterRepo,
        DomainOfInfluenceManager doiManager,
        DataContext dbContext,
        StepManager stepManager)
    {
        _repo = repo;
        _politicalBusinessVoterListEntryRepo = politicalBusinessVoterListEntryRepo;
        _auth = auth;
        _attachmentManager = attachmentManager;
        _doiRepo = doiRepo;
        _doiVoterDuplicateRepo = doiVoterDuplicateRepo;
        _clock = clock;
        _voterListImportRepo = voterListImportRepo;
        _voterRepo = voterRepo;
        _doiManager = doiManager;
        _dbContext = dbContext;
        _stepManager = stepManager;
    }

    public async Task<VoterListsData> List(Guid doiId)
    {
        var tenantId = _auth.Tenant.Id;

        var countOfEmptyVotingCards = await _doiRepo.Query()
            .WhereIsManager(tenantId)
            .Where(doi => doi.Id == doiId)
            .Select(doi => (int?)doi.CountOfEmptyVotingCards)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);

        var voterLists = await _repo
            .Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .Where(x => x.DomainOfInfluenceId == doiId)
            .Include(x => x.Import)
            .Include(x => x.PoliticalBusinessEntries!
                .OrderBy(y => y.PoliticalBusiness!.PoliticalBusinessNumber)
                .ThenBy(y => y.PoliticalBusiness!.Id))
            .Include(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!)
                .ThenInclude(x => x.PoliticalBusiness!.DomainOfInfluence)
            .Include(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!)
                .ThenInclude(x => x.PoliticalBusiness!.Translations)
            .OrderBy(x => x.Index)
            .ToListAsync();

        var voterDuplicates = await _doiVoterDuplicateRepo.Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .Where(d => d.DomainOfInfluenceId == doiId)
            .Include(d => d.Voters)
            .ToListAsync();

        var countOfVotingCardsByPoliticalBusiness = CalculateCountOfVotingCardsByPoliticalBusiness(voterLists, voterDuplicates, countOfEmptyVotingCards);

        var politicalBusinesses = voterLists
            .SelectMany(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!)
            .Select(x => x.PoliticalBusiness!)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.PoliticalBusinessNumber)
            .ThenBy(x => x.ShortDescription)
            .Select(x => new PoliticalBusinessCountOfVotingCards(x, countOfVotingCardsByPoliticalBusiness.GetValueOrDefault(x.Id, 0)))
            .ToList();

        var totalNumberOfVoters = voterLists.Sum(x => x.NumberOfVoters) + countOfEmptyVotingCards;
        var totalNumberOfVotingCards = voterLists.Sum(x => x.CountOfVotingCards) + countOfEmptyVotingCards;

        return new VoterListsData(voterLists, politicalBusinesses, totalNumberOfVoters, totalNumberOfVotingCards);
    }

    public async Task UpdateLists(
        IReadOnlyCollection<VoterListUpdateData> data)
    {
        if (data.Count == 0)
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var listIds = data.Select(vl => vl.Id).ToList();

        var existingLists = await _repo.Query()
            .Include(x => x.PoliticalBusinessEntries)
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .Where(vl => listIds.Contains(vl.Id))
            .ToListAsync();

        await EnsureValidUpdate(data, existingLists);
        var doiId = existingLists[0].DomainOfInfluenceId;
        await _stepManager.EnsureStepApproved(doiId, Step.Attachments);
        await _politicalBusinessVoterListEntryRepo.DeleteAll(listIds);

        foreach (var existingList in existingLists)
        {
            var updatedList = data.Single(vl => vl.Id == existingList.Id);

            if (updatedList.SendVotingCardsToDomainOfInfluenceReturnAddress == true && existingList.VotingCardType == VotingCardType.EVoting)
            {
                throw new ValidationException($"Cannot set {nameof(updatedList.SendVotingCardsToDomainOfInfluenceReturnAddress)} on a e-voting voter list");
            }

            existingList.PoliticalBusinessEntries = updatedList.PoliticalBusinessIds
                .Select(pbId => new PoliticalBusinessVoterListEntry
                {
                    PoliticalBusinessId = pbId,
                }).ToList();

            if (!updatedList.SendVotingCardsToDomainOfInfluenceReturnAddress.HasValue || existingList.SendVotingCardsToDomainOfInfluenceReturnAddress == updatedList.SendVotingCardsToDomainOfInfluenceReturnAddress)
            {
                continue;
            }

            var updatedSendVotingCardsToDomainOfInfluenceReturnAddress = updatedList.SendVotingCardsToDomainOfInfluenceReturnAddress.Value;

            existingList.SendVotingCardsToDomainOfInfluenceReturnAddress = updatedSendVotingCardsToDomainOfInfluenceReturnAddress;
            existingList.CountOfVotingCardsForDomainOfInfluenceReturnAddress = updatedSendVotingCardsToDomainOfInfluenceReturnAddress
                ? existingList.NumberOfVoters
                : 0;
            await _voterRepo.UpdateSendVotingCardsToDomainOfInfluenceReturnAddress(existingList.Id, updatedSendVotingCardsToDomainOfInfluenceReturnAddress);
        }

        await _repo.UpdateRange(existingLists);
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(doiId);
        await transaction.CommitAsync();
    }

    public async Task AssignPoliticalBusiness(Guid voterListId, Guid politicalBusinessId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var list = await _repo.Query()
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereDomainOfInfluenceHasPoliticalBusiness(politicalBusinessId)
            .FirstOrDefaultAsync(x => x.Id == voterListId)
            ?? throw new EntityNotFoundException(nameof(VoterList), voterListId);

        await _stepManager.EnsureStepApproved(list.DomainOfInfluenceId, Step.Attachments);

        await _politicalBusinessVoterListEntryRepo.Create(new PoliticalBusinessVoterListEntry
        {
            VoterListId = voterListId,
            PoliticalBusinessId = politicalBusinessId,
        });

        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(list.DomainOfInfluenceId);
        await transaction.CommitAsync();
    }

    public async Task UnassignPoliticalBusiness(Guid voterListId, Guid politicalBusinessId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var list = await _repo.Query()
                       .WhereContestIsNotLocked()
                       .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
                       .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
                       .Include(x => x.PoliticalBusinessEntries)
                       .FirstOrDefaultAsync(x => x.Id == voterListId)
                   ?? throw new EntityNotFoundException(nameof(VoterList), voterListId);

        await _stepManager.EnsureStepApproved(list.DomainOfInfluenceId, Step.Attachments);

        var entry = list.PoliticalBusinessEntries!.FirstOrDefault(x => x.PoliticalBusinessId == politicalBusinessId)
            ?? throw new EntityNotFoundException(nameof(PoliticalBusinessVoterListEntry), new { voterListId, politicalBusinessId });

        await _politicalBusinessVoterListEntryRepo.DeleteByKey(entry.Id);
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(list.DomainOfInfluenceId);

        await transaction.CommitAsync();
    }

    private async Task EnsureValidUpdate(IReadOnlyCollection<VoterListUpdateData> data, IReadOnlyCollection<VoterList> existingLists)
    {
        if (existingLists.Count != data.Count)
        {
            throw new ValidationException("A provided voter list was not found");
        }

        if (existingLists.Select(vl => vl.ImportId).Distinct().Count() > 1)
        {
            throw new ValidationException("You can only update voter lists of the same import at once");
        }

        var importId = existingLists.First().ImportId;

        var autoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = await _voterListImportRepo.Query()
            .Where(x => x.Id == importId)
            .Select(i => i.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit)
            .SingleAsync();

        if (autoSendVotingCardsToDomainOfInfluenceReturnAddressSplit &&
            data.Any(vl => vl.SendVotingCardsToDomainOfInfluenceReturnAddress.HasValue))
        {
            throw new ValidationException($"Cannot set {nameof(VoterList.SendVotingCardsToDomainOfInfluenceReturnAddress)} on electoral register voter lists");
        }

        var doiId = existingLists.First().DomainOfInfluenceId;

        var attendingPbIds = await _doiRepo
            .Query()
            .Where(x => x.Id == doiId && x.SecureConnectId == _auth.Tenant.Id)
            .SelectMany(x => x.PoliticalBusinessPermissionEntries!)
            .Select(x => x.PoliticalBusinessId)
            .ToListAsync();

        var assignedPbIds = data.SelectMany(vl => vl.PoliticalBusinessIds).ToHashSet();

        if (assignedPbIds.Any(pbId => !attendingPbIds.Contains(pbId)))
        {
            throw new ForbiddenException("Invalid political business found");
        }
    }

    private Dictionary<Guid, int> CalculateCountOfVotingCardsByPoliticalBusiness(List<VoterList> voterLists, List<DomainOfInfluenceVoterDuplicate> voterDuplicates, int countOfEmptyVotingCards)
    {
        var countOfVotingCardsByPoliticalBusiness = voterLists
            .SelectMany(x => x.PoliticalBusinessEntries!.Select(y => new { y.PoliticalBusinessId, x.NumberOfVoters }))
            .GroupBy(x => x.PoliticalBusinessId, x => x.NumberOfVoters)
            .ToDictionary(x => x.Key, x => x.Sum() + countOfEmptyVotingCards);

        // To get the count of voting cards of a political business we can't simply sum the "CountOfVotingCards" of the related voter lists.
        // It could be that a non related voter list contains the "effective" voter (with "VotingCardPrintDisabled"=false), which would
        // be ignored if we only consider the related voter lists.
        // To get the correct sum, we need to sum the "NumberOfVoters" of the related voter lists and subtract the duplicates.
        foreach (var pbId in countOfVotingCardsByPoliticalBusiness.Keys)
        {
            var pbVoterListIds = voterLists
                .Where(vl => vl.PoliticalBusinessEntries!.Any(e => e.PoliticalBusinessId == pbId))
                .Select(vl => vl.Id)
                .ToList();

            foreach (var voterDuplicate in voterDuplicates)
            {
                var relatedDuplicateVoterCount = voterDuplicate.Voters!
                    .Count(v => pbVoterListIds.Contains(v.ListId!.Value));

                if (relatedDuplicateVoterCount == 0)
                {
                    continue;
                }

                // The voter duplicate entity always also contains the "effective" voter (with "VotingCardPrintDisabled"=false),
                // so we always need to subtract 1 to have the effective count of involved duplicates in the related voter lists "NumberOfVoters".
                var duplicateCount = relatedDuplicateVoterCount - 1;
                countOfVotingCardsByPoliticalBusiness[pbId] -= duplicateCount;
            }
        }

        return countOfVotingCardsByPoliticalBusiness;
    }

    public record VoterListsData(
        List<VoterList> VoterLists,
        List<PoliticalBusinessCountOfVotingCards> CountOfVotingCards,
        int TotalNumberOfVoters,
        int TotalCountOfVotingCards);

    public record PoliticalBusinessCountOfVotingCards(
        PoliticalBusiness PoliticalBusiness,
        int CountOfVotingCards);
}
