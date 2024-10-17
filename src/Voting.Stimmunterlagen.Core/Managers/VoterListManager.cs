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
    private readonly PoliticalBusinessVoterListEntryRepo _politicalBusinessVoterListEntryRepo;
    private readonly IAuth _auth;
    private readonly AttachmentManager _attachmentManager;
    private readonly IClock _clock;
    private readonly IDbRepository<VoterListImport> _voterListImportRepo;
    private readonly VoterRepo _voterRepo;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _dbContext;

    public VoterListManager(
        IDbRepository<VoterList> repo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        PoliticalBusinessVoterListEntryRepo politicalBusinessVoterListEntryRepo,
        IAuth auth,
        AttachmentManager attachmentManager,
        IClock clock,
        IDbRepository<VoterListImport> voterListImportRepo,
        VoterRepo voterRepo,
        DomainOfInfluenceManager doiManager,
        DataContext dbContext)
    {
        _repo = repo;
        _politicalBusinessVoterListEntryRepo = politicalBusinessVoterListEntryRepo;
        _auth = auth;
        _attachmentManager = attachmentManager;
        _doiRepo = doiRepo;
        _clock = clock;
        _voterListImportRepo = voterListImportRepo;
        _voterRepo = voterRepo;
        _doiManager = doiManager;
        _dbContext = dbContext;
    }

    public async Task<VoterListsData> List(Guid doiId)
    {
        var tenantId = _auth.Tenant.Id;
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
            .Include(x => x.VoterDuplicates!.OrderBy(x => x.FirstName).ThenBy(x => x.LastName))
            .OrderBy(x => x.Index)
            .ToListAsync();

        var numberOfVotersByPoliticalBusiness = voterLists
            .SelectMany(x => x.PoliticalBusinessEntries!.Select(y => new { y.PoliticalBusinessId, x.NumberOfVoters }))
            .GroupBy(x => x.PoliticalBusinessId, x => x.NumberOfVoters)
            .ToDictionary(x => x.Key, x => x.Sum());

        var politicalBusinesses = voterLists
            .SelectMany(x => x.DomainOfInfluence!.PoliticalBusinessPermissionEntries!)
            .Select(x => x.PoliticalBusiness!)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.PoliticalBusinessNumber)
            .ThenBy(x => x.ShortDescription)
            .Select(x => new PoliticalBusinessNumberOfVoters(x, numberOfVotersByPoliticalBusiness.GetValueOrDefault(x.Id, 0)))
            .ToList();

        var totalNumberOfVoters = voterLists.Sum(x => x.NumberOfVoters);

        return new VoterListsData(voterLists, politicalBusinesses, totalNumberOfVoters);
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
            existingList.CountOfSendVotingCardsToDomainOfInfluenceReturnAddress = updatedSendVotingCardsToDomainOfInfluenceReturnAddress
                ? existingList.NumberOfVoters
                : 0;
            await _voterRepo.UpdateSendVotingCardsToDomainOfInfluenceReturnAddress(existingList.Id, updatedSendVotingCardsToDomainOfInfluenceReturnAddress);
        }

        await _repo.UpdateRange(existingLists);
        var doiId = existingLists[0].DomainOfInfluenceId;
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(doiId);
        await _doiManager.UpdateLastVoterUpdate(doiId);
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

        await _politicalBusinessVoterListEntryRepo.Create(new PoliticalBusinessVoterListEntry
        {
            VoterListId = voterListId,
            PoliticalBusinessId = politicalBusinessId,
        });

        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(list.DomainOfInfluenceId);
        await transaction.CommitAsync();

        // TODO: LastVoterUpdate WENN Pbs einen Einfluss auf einen Export haben
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

    public record VoterListsData(
        List<VoterList> VoterLists,
        List<PoliticalBusinessNumberOfVoters> NumberOfVoters,
        int TotalNumberOfVoters);

    public record PoliticalBusinessNumberOfVoters(
        PoliticalBusiness PoliticalBusiness,
        int NumberOfVoters);
}
