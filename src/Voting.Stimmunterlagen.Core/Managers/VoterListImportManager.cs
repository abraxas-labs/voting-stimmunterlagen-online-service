// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VoterListImportManager
{
    private const int MaxDisplayedErrorDuplicates = 20;

    private readonly IDbRepository<VoterListImport> _repo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IAuth _auth;
    private readonly IClock _clock;
    private readonly Ech0045Service _ech0045Service;
    private readonly int _voterListInsertBatchSize;
    private readonly AttachmentManager _attachmentManager;
    private readonly VoterListRepo _voterListRepo;
    private readonly IDbRepository<Voter> _voterRepo;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _dbContext;
    private readonly ILogger<VoterListImportManager> _logger;
    private readonly VoterListImportBatchHandler _voterListImportBatchHandler;
    private readonly VoterListBuilder _voterListBuilder;

    public VoterListImportManager(
        IDbRepository<VoterListImport> repo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IAuth auth,
        IClock clock,
        Ech0045Service ech0045Service,
        ApiConfig config,
        DbContextOptions dbContextOptions,
        AttachmentManager attachmentManager,
        VoterListRepo voterListRepo,
        IDbRepository<Voter> voterRepo,
        DomainOfInfluenceManager doiManager,
        DataContext dbContext,
        ILogger<VoterListImportManager> logger,
        VoterListImportBatchHandler voterListImportBatchHandler,
        VoterListBuilder voterListBuilder)
    {
        _repo = repo;
        _doiRepo = doiRepo;
        _auth = auth;
        _clock = clock;
        _ech0045Service = ech0045Service;
        _attachmentManager = attachmentManager;
        _voterListRepo = voterListRepo;
        _voterRepo = voterRepo;
        _doiManager = doiManager;
        _dbContext = dbContext;
        _logger = logger;
        _voterListImportBatchHandler = voterListImportBatchHandler;
        _voterListInsertBatchSize = config.VoterListInsertBatchSize;
        _voterListBuilder = voterListBuilder;
    }

    public async Task<VoterListImport> Get(Guid id)
    {
        return await _repo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.PoliticalBusinessEntries!.OrderBy(pb => pb.PoliticalBusiness!.PoliticalBusinessNumber))
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new EntityNotFoundException(nameof(VoterList), id);
    }

    public async Task Delete(Guid id)
    {
        var import = await _repo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new EntityNotFoundException(nameof(VoterList), id);

        await _repo.DeleteByKey(id);
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(import.DomainOfInfluenceId);
        await _doiManager.UpdateLastVoterUpdate(import.DomainOfInfluenceId);
        await _voterListBuilder.CleanUpDuplicatesAndUpdateVotingCardCountsForDomainOfInfluence(new[] { import.DomainOfInfluenceId });
    }

    public Task<VoterListImportResult> Create(VoterListImport import, XmlReader eCh0045Reader, CancellationToken ct)
        => Create(import, eCh0045Reader, null, ct);

    public Task<VoterListImportResult> Update(VoterListImport import, XmlReader eCh0045Reader, CancellationToken ct)
        => UpdateInternal(import, eCh0045Reader, ct: ct);

    public Task<VoterListImportResult> Update(VoterListImport import)
        => UpdateInternal(import);

    public Task<VoterListImportResult> Update(VoterListImport import, XmlReader eCh0045Reader, int expectedVoterCount, CancellationToken ct)
        => UpdateInternal(import, eCh0045Reader, expectedVoterCount, ct);

    public async Task<VoterListImportResult> Create(VoterListImport import, XmlReader eCh0045Reader, int? expectedVoterCount, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
        var doi = await GetDomainOfInfluence(import.DomainOfInfluenceId, ct);
        await EnsureValidCreateOrUpdate(import, null, doi);
        var existingVoterKeys = await GetExistingVoterKeys(import.DomainOfInfluenceId, ct);

        // set all pb entries on voter lists per default on create
        var pbIdsByVcType = GetAllVotingCardTypes()
            .ToDictionary(
                x => x,
                _ => doi.PoliticalBusinessPermissionEntries!.Select(pb => pb.PoliticalBusinessId).Distinct().ToList());

        var eVotingEnabled = doi.CountingCircles!.Any(x => x.CountingCircle!.EVoting) && doi.Contest!.EVoting;

        HandleVoterImportBeforeCreateVoters(import, pbIdsByVcType);
        await _repo.Create(import);
        var result = await CreateVoters(
            import,
            eCh0045Reader,
            doi.PrintData!.ShippingVotingCardsToDeliveryAddress,
            doi.ElectoralRegisterMultipleEnabled,
            doi.ContestId,
            doi.Contest!.Date,
            eVotingEnabled,
            existingVoterKeys,
            doi.VoterDuplicates!.ToList(),
            expectedVoterCount,
            ct);
        await HandleVoterImportAfterCreateVoters(import);

        if (result.Success)
        {
            await transaction.CommitAsync();
        }
        else
        {
            result.Import.Id = Guid.Empty;

            foreach (var voterList in result.Import.VoterLists!)
            {
                voterList.Id = Guid.Empty;
            }
        }

        return result;
    }

    private async Task<VoterListImportResult> UpdateInternal(VoterListImport import, XmlReader? eCh0045Reader = null, int? expectedVoterCount = null, CancellationToken ct = default)
    {
        var tenantId = _auth.Tenant.Id;

        var existingImport = await _repo.Query()
            .Include(x => x.VoterLists!)
            .ThenInclude(x => x.PoliticalBusinessEntries)
            .WhereIsDomainOfInfluenceManager(tenantId)
            .FirstOrDefaultAsync(x => x.Id == import.Id, ct)
            ?? throw new EntityNotFoundException(nameof(VoterListImport), import.Id);

        import.DomainOfInfluenceId = existingImport.DomainOfInfluenceId;

        var doi = await GetDomainOfInfluence(import.DomainOfInfluenceId, ct);
        await EnsureValidCreateOrUpdate(import, existingImport, doi);

        var eVotingEnabled = doi.CountingCircles!.Any(x => x.CountingCircle!.EVoting) && doi.Contest!.EVoting;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        if (eCh0045Reader == null)
        {
            import.SourceId = existingImport.SourceId;
            import.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = existingImport.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit;
            await _repo.Update(import);
            await transaction.CommitAsync();
            return new VoterListImportResult();
        }

        var pbIdsByVcType = existingImport.VoterLists!.ToDictionary(
            vl => vl.VotingCardType,
            vl => vl.PoliticalBusinessEntries!.Select(pbe => pbe.PoliticalBusinessId).ToList());

        await _voterListRepo.DeleteRangeByKey(existingImport.VoterLists!.Select(vl => vl.Id).ToList());
        var existingVoterKeys = await GetExistingVoterKeys(import.DomainOfInfluenceId, ct);

        HandleVoterImportBeforeCreateVoters(import, pbIdsByVcType);
        await _repo.Update(import);
        var result = await CreateVoters(
            import,
            eCh0045Reader,
            doi.PrintData!.ShippingVotingCardsToDeliveryAddress,
            doi.ElectoralRegisterMultipleEnabled,
            doi.ContestId,
            doi.Contest!.Date,
            eVotingEnabled,
            existingVoterKeys,
            doi.VoterDuplicates!.ToList(),
            expectedVoterCount,
            ct);
        await HandleVoterImportAfterCreateVoters(import);

        if (result.Success)
        {
            await transaction.CommitAsync();
        }

        return result;
    }

    /// <summary>
    /// Add voter lists to import. Is needed for performance reasons, so that the voters can be inserted
    /// efficiently by just adding the foreign key list id.
    /// </summary>
    /// <param name="import">The Voter list import.</param>
    /// <param name="politicalBusinessIdsByVotingCardType">Existing political business ids by voting card type.</param>
    private void HandleVoterImportBeforeCreateVoters(VoterListImport import, Dictionary<VotingCardType, List<Guid>>? politicalBusinessIdsByVotingCardType = null)
    {
        import.VoterLists = new List<VoterList>();

        foreach (var type in GetAllVotingCardTypes())
        {
            var pbIds = politicalBusinessIdsByVotingCardType?.GetValueOrDefault(type) ?? new();

            import.VoterLists.Add(new VoterList
            {
                VotingCardType = type,
                DomainOfInfluenceId = import.DomainOfInfluenceId,
                PoliticalBusinessEntries = pbIds.ConvertAll(pbId => new PoliticalBusinessVoterListEntry { PoliticalBusinessId = pbId }),
            });
        }
    }

    private async Task HandleVoterImportAfterCreateVoters(VoterListImport import)
    {
        var emptyVoterListIds = import.VoterLists!
            .Where(vl => vl.NumberOfVoters == 0)
            .Select(vl => vl.Id)
            .ToList();

        var voterLists = import.VoterLists!.Where(vl => !emptyVoterListIds.Contains(vl.Id)).ToList();
        if (voterLists.Count == 0)
        {
            throw new ValidationException("Cannot create an import without any voters");
        }

        var maxIndex = await GetMaxVoterListIndex(import.DomainOfInfluenceId);

        foreach (var voterList in voterLists)
        {
            voterList.Index = ++maxIndex;
        }

        // update the voter counts of all voter lists
        // and remove the empty voter lists.
        if (emptyVoterListIds.Count > 0)
        {
            await _voterListRepo.DeleteRangeByKey(emptyVoterListIds);
        }

        await _voterListRepo.UpdateRangeIgnoreRelations(voterLists);
        await _voterListRepo.UpdateVotingCardCounts(import.DomainOfInfluenceId);
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(import.DomainOfInfluenceId);
        await _doiManager.UpdateLastVoterUpdate(import.DomainOfInfluenceId);
        import.VoterLists = await _voterListRepo.Query()
            .Where(vl => vl.ImportId == import.Id)
            .OrderBy(vl => vl.VotingCardType)
            .ToListAsync();
    }

    private async Task<ContestDomainOfInfluence> GetDomainOfInfluence(Guid doiId, CancellationToken ct)
    {
        return await _doiRepo.Query()
            .WhereContestNotLocked()
            .WhereContestIsNotPastGenerateVotingCardsDeadline(_clock)
            .WhereIsManager(_auth.Tenant.Id)
            .Where(x => x.Id == doiId)
            .Include(doi => doi.PoliticalBusinessPermissionEntries)
            .Include(doi => doi.CountingCircles!)
            .ThenInclude(x => x.CountingCircle)
            .Include(x => x.Contest)
            .Include(x => x.VoterDuplicates)
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
    }

    private async Task<VoterListImportResult> CreateVoters(
        VoterListImport import,
        XmlReader eCh0045Reader,
        bool shippingVotingCardsToDeliveryAddress,
        bool electoralRegisterMultipleEnabled,
        Guid contestId,
        DateTime contestDate,
        bool eVotingEnabled,
        Dictionary<VoterKey, List<Guid>> existingVoterIdsByVoterKey,
        List<DomainOfInfluenceVoterDuplicate> existingVoterDuplicates,
        int? expectedVoterCount,
        CancellationToken ct)
    {
        var voters = _ech0045Service.ReadVoters(Ech0045Version.V4, eCh0045Reader, shippingVotingCardsToDeliveryAddress, eVotingEnabled, ct);
        var listByVcType = import.VoterLists!.ToDictionary(x => x.VotingCardType, x => x);

        var voterDuplicatesBuilder = new VoterDuplicatesBuilder(
            import.DomainOfInfluenceId,
            existingVoterDuplicates,
            existingVoterIdsByVoterKey);

        var voterHouseholdBuilder = new VoterHouseholdBuilder(import);
        var errorDuplicates = new HashSet<VoterDuplicateKey>();

        // manual uploads which are not from Stimmregister set the bool on the list itself (which then gets propagated on the voter), and not directly on the voter.
        var ignoreSendVotingCardsToDoiReturnAddress = !import.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit;

        await foreach (var voterChunk in voters.Chunked(_voterListInsertBatchSize).WithCancellation(ct))
        {
            await _voterListImportBatchHandler.CreateVoters(
                voterChunk,
                listByVcType,
                contestId,
                contestDate,
                ignoreSendVotingCardsToDoiReturnAddress,
                electoralRegisterMultipleEnabled,
                voterDuplicatesBuilder,
                voterHouseholdBuilder,
                errorDuplicates,
                ct);
        }

        await UpdateHouseholders(import, voterHouseholdBuilder, ct);

        var voterCounter = import.VoterLists!.Sum(vl => vl.NumberOfVoters);

        if (expectedVoterCount.HasValue && voterCounter != expectedVoterCount)
        {
            throw new InvalidOperationException($"Expected voter count {expectedVoterCount} does not match actual imported voters {voterCounter}");
        }

        return new VoterListImportResult
        {
            Import = import,
            VoterDuplicates = errorDuplicates.Count <= MaxDisplayedErrorDuplicates
                ? errorDuplicates.ToList()
                : new(),
            VoterDuplicatesCount = errorDuplicates.Count,
            Success = errorDuplicates.Count == 0,
        };
    }

    private async Task UpdateHouseholders(VoterListImport import, VoterHouseholdBuilder voterHouseholdBuilder, CancellationToken ct)
    {
        var listById = import.VoterLists!.ToDictionary(x => x.Id, x => x);

        foreach (var households in voterHouseholdBuilder.GetHouseholdsByListId())
        {
            var votersToUpdate = households.Value.Values.Where(v => !v.IsHouseholder).Select(v => v.PersonId).ToList();

            if (votersToUpdate.Count == 0)
            {
                continue;
            }

            await _dbContext.Voters
                .Where(v => v.ListId.Equals(households.Key) && votersToUpdate.Contains(v.PersonId))
                .ExecuteUpdateAsync(v => v.SetProperty(e => e.IsHouseholder, true), cancellationToken: ct);
        }
    }

    private async Task EnsureValidCreateOrUpdate(VoterListImport import, VoterListImport? existingImport, ContestDomainOfInfluence doi)
    {
        if (!Enum.IsDefined(typeof(VoterListSource), import.Source))
        {
            throw new ValidationException("Unknown voter list source value");
        }

        if (existingImport != null && existingImport.Source != import.Source)
        {
            throw new ValidationException("Cannot edit the source of a voter list");
        }

        if (import.Source == VoterListSource.ManualEch45Upload && !doi.CanManuallyUploadVoterList)
        {
            throw new ForbiddenException($"Cannot manually upload a voter list for {doi.Name}/{doi.Id}");
        }

        if (import.Source == VoterListSource.VotingStimmregisterFilterVersion && !doi.ElectoralRegistrationEnabled)
        {
            throw new ForbiddenException($"Cannot upload a voter list from an electoral register when the electoral register is disabled for {doi.Canton}/{doi.Name}/{doi.Id}");
        }

        if (!doi.ElectoralRegisterMultipleEnabled && import.Source == VoterListSource.VotingStimmregisterFilterVersion)
        {
            var importsDeletedCount = await _repo.Query()
                .Where(i => i.DomainOfInfluenceId == doi.Id && i.Id != import.Id && i.Source == VoterListSource.VotingStimmregisterFilterVersion)
                .ExecuteDeleteAsync();

            if (importsDeletedCount > 0)
            {
                _logger.LogInformation(
                    "Domain of influence {Id} has no multiple electoral registers enabled and deleted {Count} existing voter list imports.",
                    doi.Id,
                    importsDeletedCount);
            }
        }
    }

    private async Task<Dictionary<VoterKey, List<Guid>>> GetExistingVoterKeys(Guid doiId, CancellationToken ct)
    {
        var items = await _voterRepo.Query()
            .Where(v => v.List!.DomainOfInfluenceId == doiId)
            .Select(v => new { v.Id, Key = new VoterKey(v.FirstName, v.LastName, v.DateOfBirth, v.Street, v.HouseNumber) })
            .ToListAsync(ct);

        return items
            .GroupBy(i => i.Key)
            .ToDictionary(i => i.Key, i => i.Select(k => k.Id).ToList());
    }

    private async Task<int> GetMaxVoterListIndex(Guid domainOfInfluenceId)
    {
        return await _voterListRepo.Query()
            .Where(x => x.DomainOfInfluenceId == domainOfInfluenceId)
            .MaxAsync(x => (int?)x.Index) ?? 0;
    }

    private VotingCardType[] GetAllVotingCardTypes() =>
        new[] { VotingCardType.Swiss, VotingCardType.SwissAbroad, VotingCardType.EVoting };
}
