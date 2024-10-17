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
using Microsoft.EntityFrameworkCore.Storage;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Voting.Stimmunterlagen.Core.Managers;

public class VoterListImportManager
{
    private readonly IDbRepository<VoterListImport> _repo;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IAuth _auth;
    private readonly IClock _clock;
    private readonly EchService _echService;
    private readonly int _voterListInsertBatchSize;
    private readonly DbContextOptions _dbContextOptions;
    private readonly AttachmentManager _attachmentManager;
    private readonly IDbRepository<VoterList> _voterListRepo;
    private readonly IDbRepository<VoterDuplicate> _voterDuplicateRepo;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _dbContext;

    public VoterListImportManager(
        IDbRepository<VoterListImport> repo,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IAuth auth,
        IClock clock,
        EchService echService,
        ApiConfig config,
        DbContextOptions dbContextOptions,
        AttachmentManager attachmentManager,
        IDbRepository<VoterList> voterListRepo,
        IDbRepository<VoterDuplicate> voterDuplicateRepo,
        DomainOfInfluenceManager doiManager,
        DataContext dbContext)
    {
        _repo = repo;
        _doiRepo = doiRepo;
        _auth = auth;
        _clock = clock;
        _echService = echService;
        _voterListInsertBatchSize = config.VoterListInsertBatchSize;
        _dbContextOptions = dbContextOptions;
        _attachmentManager = attachmentManager;
        _voterListRepo = voterListRepo;
        _voterDuplicateRepo = voterDuplicateRepo;
        _doiManager = doiManager;
        _dbContext = dbContext;
    }

    public async Task<VoterListImport> Get(Guid id)
    {
        return await _repo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.PoliticalBusinessEntries!.OrderBy(pb => pb.PoliticalBusiness!.PoliticalBusinessNumber))
            .Include(x => x.VoterLists!.OrderBy(vl => vl.VotingCardType))
            .ThenInclude(x => x.VoterDuplicates!.OrderBy(d => d.LastName).ThenBy(d => d.FirstName))
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
    }

    public Task Create(VoterListImport import, XmlReader eCh0045Reader, CancellationToken ct)
        => Create(import, eCh0045Reader, null, ct);

    public Task Update(VoterListImport import, XmlReader eCh0045Reader, CancellationToken ct)
        => UpdateInternal(import, eCh0045Reader, ct: ct);

    public Task Update(VoterListImport import)
        => UpdateInternal(import);

    public Task Update(VoterListImport import, XmlReader eCh0045Reader, int expectedVoterCount, CancellationToken ct)
        => UpdateInternal(import, eCh0045Reader, expectedVoterCount, ct);

    public async Task Create(VoterListImport import, XmlReader eCh0045Reader, int? expectedVoterCount, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var doi = await GetDomainOfInfluence(import.DomainOfInfluenceId, ct);
        EnsureValidCreateOrUpdate(import, null, doi);

        // set all pb entries on voter lists per default on create
        var pbIdsByVcType = GetAllVotingCardTypes()
            .ToDictionary(
                x => x,
                _ => doi.PoliticalBusinessPermissionEntries!.Select(pb => pb.PoliticalBusinessId).Distinct().ToList());

        var eVotingEnabled = doi.CountingCircles!.Any(x => x.CountingCircle!.EVoting) && doi.Contest!.EVoting;

        HandleVoterImportBeforeCreateVoters(import, pbIdsByVcType);
        await _repo.Create(import);
        await CreateVoters(import, eCh0045Reader, doi.PrintData!.ShippingVotingCardsToDeliveryAddress, doi.ContestId, eVotingEnabled, expectedVoterCount, ct);
        await HandleVoterImportAfterCreateVoters(import);
        await transaction.CommitAsync();
    }

    private async Task UpdateInternal(VoterListImport import, XmlReader? eCh0045Reader = null, int? expectedVoterCount = null, CancellationToken ct = default)
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
        EnsureValidCreateOrUpdate(import, existingImport, doi);

        var eVotingEnabled = doi.CountingCircles!.Any(x => x.CountingCircle!.EVoting) && doi.Contest!.EVoting;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        if (eCh0045Reader == null)
        {
            import.SourceId = existingImport.SourceId;
            import.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit = existingImport.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit;
            await _repo.Update(import);
            await transaction.CommitAsync();
            return;
        }

        var pbIdsByVcType = existingImport.VoterLists!.ToDictionary(
            vl => vl.VotingCardType,
            vl => vl.PoliticalBusinessEntries!.Select(pbe => pbe.PoliticalBusinessId).ToList());

        await _voterListRepo.DeleteRangeByKey(existingImport.VoterLists!.Select(vl => vl.Id).ToList());

        HandleVoterImportBeforeCreateVoters(import, pbIdsByVcType);
        await _repo.Update(import);
        await CreateVoters(import, eCh0045Reader, doi.PrintData!.ShippingVotingCardsToDeliveryAddress, doi.ContestId, eVotingEnabled, expectedVoterCount, ct);
        await HandleVoterImportAfterCreateVoters(import);
        await transaction.CommitAsync();
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
                VoterDuplicates = new List<VoterDuplicate>(),
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

        await _voterDuplicateRepo.CreateRange(voterLists.SelectMany(l => l.VoterDuplicates!));
        await _voterListRepo.UpdateRangeIgnoreRelations(voterLists);
        await _attachmentManager.UpdateRequiredCountForDomainOfInfluence(import.DomainOfInfluenceId);
        await _doiManager.UpdateLastVoterUpdate(import.DomainOfInfluenceId);
        import.VoterLists = voterLists;
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
            .FirstOrDefaultAsync(ct)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
    }

    private async Task CreateVoters(
        VoterListImport import,
        XmlReader eCh0045Reader,
        bool shippingVotingCardsToDeliveryAddress,
        Guid contestId,
        bool eVotingEnabled,
        int? expectedVoterCount,
        CancellationToken ct)
    {
        var voters = _echService.ReadVoters(eCh0045Reader, shippingVotingCardsToDeliveryAddress, eVotingEnabled, ct);
        var listByVcType = import.VoterLists!.ToDictionary(x => x.VotingCardType, x => x);
        var voterDuplicatesBuilder = new VoterDuplicatesBuilder(import);

        // manual uploads which are not from Stimmregister set the bool on the list itself (which then gets propagated on the voter), and not directly on the voter.
        var ignoreSendVotingCardsToDoiReturnAddress = !import.AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit;

        await foreach (var voterChunk in voters.Chunked(_voterListInsertBatchSize).WithCancellation(ct))
        {
            foreach (var voter in voterChunk)
            {
                var list = listByVcType[voter.VotingCardType];
                voter.ListId = list.Id;
                voter.ContestId = contestId;
                list.NumberOfVoters++;

                if (voter.SendVotingCardsToDomainOfInfluenceReturnAddress)
                {
                    if (ignoreSendVotingCardsToDoiReturnAddress)
                    {
                        voter.SendVotingCardsToDomainOfInfluenceReturnAddress = false;
                    }
                    else
                    {
                        list.CountOfSendVotingCardsToDomainOfInfluenceReturnAddress++;
                    }
                }

                voterDuplicatesBuilder.NextVoter(voter);
            }

            // Because we are inserting a lot of voters (biggest domain of influence may have above 100k voters)
            // we need to take care that we do not allocate too much memory.
            // Using the same DbContext for all saves does not perform well, as all entries are being "cached".
            // Creating a new DbContext for each batch allows the GC to clean up data related to previous batches
            await using var dbContext = new DataContext(_dbContextOptions);
            dbContext.Database.SetDbConnection(_dbContext.Database.GetDbConnection());
            await dbContext.Database.UseTransactionAsync(_dbContext.Database.CurrentTransaction!.GetDbTransaction(), ct);
            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            dbContext.Voters.AddRange(voterChunk);
            await dbContext.SaveChangesAsync(ct);
        }

        var voterCounter = import.VoterLists!.Sum(vl => vl.NumberOfVoters);

        if (expectedVoterCount.HasValue && voterCounter != expectedVoterCount)
        {
            throw new InvalidOperationException($"Expected voter count {expectedVoterCount} does not match actual imported voters {voterCounter}");
        }
    }

    private void EnsureValidCreateOrUpdate(VoterListImport import, VoterListImport? existingImport, ContestDomainOfInfluence doi)
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
