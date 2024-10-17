// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Exceptions;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Data.Utils;

namespace Voting.Stimmunterlagen.Core.Managers;

public class AttachmentManager
{
    private readonly IAuth _auth;
    private readonly AttachmentRepo _attachmentRepo;
    private readonly PoliticalBusinessAttachmentEntryRepo _politicalBusinessAttachmentEntryRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _domainOfInfluenceRepo;
    private readonly DomainOfInfluenceAttachmentCountRepo _domainOfInfluenceAttachmentCountRepo;
    private readonly IDbRepository<AttachmentComment> _attachmentCommentRepo;
    private readonly IClock _clock;
    private readonly UserManager _userManager;
    private readonly PrintJobBuilder _printJobBuilder;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly AttachmentCategorySummaryBuilder _summaryBuilder;
    private readonly ContestManager _contestManager;
    private readonly DataContext _dbContext;

    public AttachmentManager(
        IAuth auth,
        AttachmentRepo attachmentRepo,
        PoliticalBusinessAttachmentEntryRepo politicalBusinessAttachmentEntryRepo,
        IDbRepository<ContestDomainOfInfluence> domainOfInfluenceRepo,
        DomainOfInfluenceAttachmentCountRepo domainOfInfluenceAttachmentCountRepo,
        IDbRepository<AttachmentComment> attachmentCommentRepo,
        IClock clock,
        UserManager userManager,
        PrintJobBuilder printJobBuilder,
        DomainOfInfluenceManager doiManager,
        AttachmentCategorySummaryBuilder summaryBuilder,
        ContestManager contestManager,
        DataContext dbContext)
    {
        _auth = auth;
        _attachmentRepo = attachmentRepo;
        _politicalBusinessAttachmentEntryRepo = politicalBusinessAttachmentEntryRepo;
        _domainOfInfluenceRepo = domainOfInfluenceRepo;
        _domainOfInfluenceAttachmentCountRepo = domainOfInfluenceAttachmentCountRepo;
        _attachmentCommentRepo = attachmentCommentRepo;
        _clock = clock;
        _userManager = userManager;
        _printJobBuilder = printJobBuilder;
        _doiManager = doiManager;
        _summaryBuilder = summaryBuilder;
        _contestManager = contestManager;
        _dbContext = dbContext;
    }

    public async Task<List<AttachmentCategorySummary>> ListCategorySummariesForFilter(Guid contestId, string queryString, AttachmentState? state, bool forPrintJobManagement)
    {
        var escapedLikeQuery = $"%{SqlUtils.EscapeLike(queryString)}%";

        var query = _attachmentRepo.Query()
            .Include(a => a.DomainOfInfluence)
            .Include(a => a.PoliticalBusinessEntries)
            .Where(a => a.DomainOfInfluence!.ContestId == contestId
                && (state == null || a.State == state)
                && (string.IsNullOrEmpty(queryString)
                    || EF.Functions.ILike(a.Name, escapedLikeQuery, SqlUtils.DefaultEscapeCharacter)
                    || EF.Functions.ILike(a.DomainOfInfluence.AuthorityName, escapedLikeQuery, SqlUtils.DefaultEscapeCharacter)));

        if (!forPrintJobManagement)
        {
            query = query.WhereIsContestManager(_auth.Tenant.Id);
        }

        var attachments = await query.ToListAsync();
        return await _summaryBuilder.BuildForContest(attachments, contestId);
    }

    public async Task<List<DomainOfInfluenceAttachmentCategorySummariesEntry>> ListDomainOfInfluenceAttachmentCategorySummaries(Guid domainOfInfluenceId)
    {
        // tenant check is done in the list for domain of influence method.
        var attachments = await ListForDomainOfInfluence(domainOfInfluenceId, true);
        var attachmentCategorySummariesByDoiId = await _summaryBuilder.BuildGroupedByDomainOfInfluence(attachments, domainOfInfluenceId);

        // add the request doi id explicitly, because it is possible that there is no attachment on it yet.
        attachmentCategorySummariesByDoiId.TryAdd(domainOfInfluenceId, new());

        var doiIds = attachmentCategorySummariesByDoiId.Keys.ToList();
        var dois = await _domainOfInfluenceRepo.Query()
            .Include(doi => doi.PoliticalBusinessPermissionEntries!)
            .ThenInclude(x => x.PoliticalBusiness!.Translations)
            .Include(doi => doi.PoliticalBusinessPermissionEntries!)
            .ThenInclude(x => x.PoliticalBusiness!.DomainOfInfluence)
            .Where(doi => doiIds.Contains(doi.Id) && !doi.ExternalPrintingCenter)
            .OrderBy(doi => doi.Type)
            .ThenBy(doi => doi.Name)
            .ToListAsync();

        if (dois.Count == 0)
        {
            return new();
        }

        return dois
            .ConvertAll(doi => new DomainOfInfluenceAttachmentCategorySummariesEntry(
                doi,
                attachmentCategorySummariesByDoiId.GetValueOrDefault(doi.Id) ?? new(),
                doi.PoliticalBusinessPermissionEntries!
                    .Select(x => x.PoliticalBusiness!)
                    .DistinctBy(x => x.Id)
                    .OrderBy(x => x.PoliticalBusinessNumber)
                    .ThenBy(x => x.ShortDescription)
                    .ToList()));
    }

    public async Task<List<AttachmentCategorySummary>> ListCategorySummariesForDomainOfInfluence(Guid domainOfInfluenceId, bool forCurrentTenant)
    {
        var isContestManager = await _domainOfInfluenceRepo.Query()
            .WhereIsContestManager(_auth.Tenant.Id)
            .AnyAsync(doi => doi.Id == domainOfInfluenceId);

        // a contest manager should be able to view attachments from his own contest, even if for current tenant is true
        if (forCurrentTenant && isContestManager)
        {
            forCurrentTenant = false;
        }

        var attachments = await ListForDomainOfInfluence(domainOfInfluenceId, forCurrentTenant);
        return await _summaryBuilder.BuildForDomainOfInfluence(attachments, domainOfInfluenceId);
    }

    public async Task<List<DomainOfInfluenceAttachmentCount>> ListDomainOfInfluenceAttachmentCounts(Guid attachmentId)
    {
        var existingAttachment = await _attachmentRepo.Query()
            .Include(a => a.DomainOfInfluenceAttachmentCounts!.OrderBy(c => c.DomainOfInfluence!.Name))
            .ThenInclude(c => c.DomainOfInfluence)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        return existingAttachment?.DomainOfInfluenceAttachmentCounts!.ToList() ?? new();
    }

    public async Task<bool> HasAttachmentsStationSet(Guid contestId)
    {
        await _contestManager.EnsureIsContestManager(contestId);

        var eVotingDoiIds = await _domainOfInfluenceRepo.Query()
            .WhereIsInEVotingExport()
            .Where(doi => doi.ContestId == contestId)
            .Select(doi => doi.Id)
            .ToListAsync();

        return await _attachmentRepo.Query()
            .Where(a => (a.State == AttachmentState.Defined || a.State == AttachmentState.Delivered)
                && a.DomainOfInfluenceAttachmentCounts!.Any(doiAc => eVotingDoiIds.Contains(doiAc.DomainOfInfluenceId)
                    && doiAc.RequiredCount != 0)) // if > 0, then the attachment is required by the doi, if null the state is status quo (the doi has not chosen yet)
            .AllAsync(a => a.Station != null);
    }

    public async Task<Guid> Create(Attachment attachment, int requiredCount)
    {
        await EnsureValidCreateOrUpdate(attachment, requiredCount);
        await AddDomainOfInfluenceAttachmentCounts(attachment, requiredCount);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await _attachmentRepo.Create(attachment);

        var doi = await _domainOfInfluenceRepo.Query()
                      .Include(x => x.Contest)
                      .SingleOrDefaultAsync(x => x.Id == attachment.DomainOfInfluenceId)
                  ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), attachment.DomainOfInfluenceId);

        // voter lists could already have been imported
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(attachmentId: attachment.Id, isPoliticalAssembly: doi.Contest!.IsPoliticalAssembly);
        await _attachmentRepo.UpdateTotalCounts(attachment.Id);
        await transaction.CommitAsync();

        return attachment.Id;
    }

    public async Task Update(Attachment attachment, int requiredCount)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var existingAttachment = await _attachmentRepo.Query()
            .Include(a => a.DomainOfInfluence!.Contest)
            .Include(a => a.DomainOfInfluenceAttachmentCounts)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .FirstOrDefaultAsync(a => a.Id == attachment.Id)
            ?? throw new EntityNotFoundException(nameof(Attachment), attachment.Id);

        attachment.DomainOfInfluenceId = existingAttachment.DomainOfInfluenceId;
        await EnsureValidCreateOrUpdate(attachment, requiredCount);

        await SetDomainOfInfluenceAttachmentRequiredCount(existingAttachment, existingAttachment.DomainOfInfluenceId, requiredCount);
        await _politicalBusinessAttachmentEntryRepo.Replace(attachment.Id, attachment.PoliticalBusinessEntries!);
        await _attachmentRepo.Update(attachment);
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(attachmentId: attachment.Id, isPoliticalAssembly: attachment.DomainOfInfluence!.Contest!.IsPoliticalAssembly);
        await _attachmentRepo.UpdateTotalCounts(attachment.Id);

        await transaction.CommitAsync();
    }

    public async Task Delete(Guid id)
    {
        var attachment = await _attachmentRepo.Query()
            .Include(x => x.DomainOfInfluence!.Contest)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastPrintingCenterSignUpDeadline(_clock)
            .SingleOrDefaultAsync(a => a.Id == id)
                ?? throw new EntityNotFoundException(nameof(Attachment), id);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await _attachmentRepo.DeleteByKey(id);
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(attachmentId: id, isPoliticalAssembly: attachment.DomainOfInfluence!.Contest!.IsPoliticalAssembly);
        await _attachmentRepo.UpdateTotalCounts(id);
        await transaction.CommitAsync();
    }

    public async Task AssignPoliticalBusiness(Guid attachmentId, Guid politicalBusinessId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await EnsureCanAssignPoliticalBusiness(attachmentId, politicalBusinessId);

        await _politicalBusinessAttachmentEntryRepo.Create(new PoliticalBusinessAttachmentEntry
        {
            AttachmentId = attachmentId,
            PoliticalBusinessId = politicalBusinessId,
        });
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(attachmentId: attachmentId);
        await _attachmentRepo.UpdateTotalCounts(attachmentId);
        await transaction.CommitAsync();
    }

    public async Task UnassignPoliticalBusiness(Guid attachmentId, Guid politicalBusinessId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await EnsureCanAssignPoliticalBusiness(attachmentId, politicalBusinessId);

        var existingEntry = await _politicalBusinessAttachmentEntryRepo.Query()
                                .SingleOrDefaultAsync(x =>
                                    x.AttachmentId == attachmentId && x.PoliticalBusinessId == politicalBusinessId)
                            ?? throw new EntityNotFoundException(nameof(PoliticalBusinessAttachmentEntry), new { attachmentId, politicalBusinessId });

        await _politicalBusinessAttachmentEntryRepo.DeleteByKey(existingEntry.Id);
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(attachmentId: attachmentId);
        await _attachmentRepo.UpdateTotalCounts(attachmentId);
        await transaction.CommitAsync();
    }

    public async Task SetDomainOfInfluenceAttachmentRequiredCount(Guid attachmentId, Guid domainOfInfluenceId, int requiredCount)
    {
        var tenantId = _auth.Tenant.Id;

        var contestDoiExistsAndEditable = await _domainOfInfluenceRepo.Query()
            .WhereContestNotLocked()
            .WhereContestIsNotPastPrintingCenterSignUpDeadline(_clock)
            .WhereIsManager(tenantId)
            .AnyAsync(x => x.Id == domainOfInfluenceId);

        if (!contestDoiExistsAndEditable)
        {
            throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), domainOfInfluenceId);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        var attachment = await _attachmentRepo.Query()
            .WhereCanSetDomainOfInfluenceCount(tenantId, domainOfInfluenceId)
            .Include(x => x.DomainOfInfluence!.Contest)
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstOrDefaultAsync(x => x.Id == attachmentId)
            ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceAttachmentCount), new { attachmentId, domainOfInfluenceId });

        if (!attachment.DomainOfInfluence!.ResponsibleForVotingCards && attachment.DomainOfInfluenceId == domainOfInfluenceId)
        {
            throw new ForbiddenException("cannot set attachment count when not responsible for voting cards");
        }

        if (attachment.DomainOfInfluenceId == domainOfInfluenceId)
        {
            EnsureValidAttachmentCounts(attachment, attachment.DomainOfInfluence.Type, requiredCount);
        }

        await SetDomainOfInfluenceAttachmentRequiredCount(attachment, domainOfInfluenceId, requiredCount);
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(domainOfInfluenceId, attachmentId, attachment.DomainOfInfluence!.Contest!.IsPoliticalAssembly);
        await _attachmentRepo.UpdateTotalCounts(attachmentId);
        await transaction.CommitAsync();
    }

    public async Task UpdateDomainOfInfluenceEntries(Guid attachmentId, List<Guid> domainOfInfluenceIds)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var existingAttachment = await _attachmentRepo.Query()
            .Include(a => a.DomainOfInfluence)
            .Include(a => a.DomainOfInfluenceAttachmentCounts)
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotPastPrintingCenterSignUpDeadline(_clock)
            .WhereContestIsNotLocked()
            .FirstOrDefaultAsync(a => a.Id == attachmentId)
            ?? throw new EntityNotFoundException(nameof(Attachment), attachmentId);

        // the count of the attachment owner is always set.
        existingAttachment.DomainOfInfluenceAttachmentCounts = existingAttachment.DomainOfInfluenceAttachmentCounts!
            .Where(x => existingAttachment.DomainOfInfluenceId != x.DomainOfInfluenceId)
            .ToList();

        await EnsureAssignableDomainOfInfluenceIds(existingAttachment, domainOfInfluenceIds);

        var existingDoiAttachmentCountIds = existingAttachment.DomainOfInfluenceAttachmentCounts!
            .Select(x => x.DomainOfInfluenceId)
            .ToList();

        var toRemove = existingAttachment.DomainOfInfluenceAttachmentCounts!
            .Where(x => !domainOfInfluenceIds.Contains(x.DomainOfInfluenceId))
            .Select(x => x.Id)
            .ToList();

        var toAdd = domainOfInfluenceIds
            .Where(doiId => !existingDoiAttachmentCountIds.Contains(doiId))
            .Select(doiId => new DomainOfInfluenceAttachmentCount { AttachmentId = attachmentId, DomainOfInfluenceId = doiId })
            .ToList();

        await _domainOfInfluenceAttachmentCountRepo.DeleteRangeByKey(toRemove);
        await _domainOfInfluenceAttachmentCountRepo.CreateRange(toAdd);
        await _attachmentRepo.UpdateTotalCounts(attachmentId);
        await transaction.CommitAsync();
    }

    public async Task SetStation(Guid attachmentId, int station)
    {
        var existingAttachment = await _attachmentRepo.Query()
            .WhereContestIsNotLocked()
            .WhereDomainOfInfluenceIsNotExternalPrintingCenter()
            .Include(a => a.DomainOfInfluence)
            .FirstOrDefaultAsync(a => a.Id == attachmentId)
            ?? throw new EntityNotFoundException(nameof(Attachment), attachmentId);

        existingAttachment.Station = station;
        await _attachmentRepo.Update(existingAttachment);
    }

    public async Task SetState(Guid attachmentId, AttachmentState state, string commentContent)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var existingAttachment = await _attachmentRepo.Query()
            .WhereContestIsNotLocked()
            .WhereDomainOfInfluenceIsNotExternalPrintingCenter()
            .Include(x => x.DomainOfInfluence)
            .FirstOrDefaultAsync(a => a.Id == attachmentId)
            ?? throw new EntityNotFoundException(nameof(Attachment), attachmentId);

        existingAttachment.State = state;
        existingAttachment.DeliveryReceivedOn = existingAttachment.State == AttachmentState.Delivered
            ? _clock.UtcNow
            : null;

        await _attachmentRepo.Update(existingAttachment);
        await CreateCommentIfNeeded(attachmentId, commentContent);
        await _printJobBuilder.SyncStateForDomainOfInfluence(
            existingAttachment.DomainOfInfluenceId,
            await GetAllRequiredAttachmentsByDomainOfInfluenceId(existingAttachment.DomainOfInfluence!.ContestId));

        await transaction.CommitAsync();
    }

    internal async Task<List<Attachment>> ListForDomainOfInfluence(Guid domainOfInfluenceId, bool forCurrentTenant)
    {
        var doi = await _domainOfInfluenceRepo.Query()
            .Include(doi => doi.PoliticalBusinesses)
            .FirstOrDefaultAsync(doi => doi.Id == domainOfInfluenceId);

        if (doi == null)
        {
            return new();
        }

        var tenantId = forCurrentTenant
            ? _auth.Tenant.Id
            : await _doiManager.GetSecureConnectId(domainOfInfluenceId);

        var managedPbIds = doi.PoliticalBusinesses!.Select(pb => pb.Id).ToList();

        return await _attachmentRepo.Query()
            .Include(a => a.DomainOfInfluence)
            .Include(a => a.PoliticalBusinessEntries!
                .OrderBy(x => x.PoliticalBusiness!.PoliticalBusinessNumber)
                .ThenBy(x => x.PoliticalBusiness!.Id))
            .Include(a => a.DomainOfInfluenceAttachmentCounts!
                .Where(x => x.DomainOfInfluenceId == domainOfInfluenceId))
            .WhereHasAccess(tenantId, domainOfInfluenceId, managedPbIds)
            .OrderBy(a => a.DomainOfInfluence!.Type)
            .ThenBy(a => a.DomainOfInfluence!.Name)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    internal async Task UpdateRequiredCountForDomainOfInfluence(Guid doiId)
    {
        var doi = await _domainOfInfluenceRepo.Query()
            .Include(x => x.Contest)
            .SingleOrDefaultAsync(x => x.Id == doiId)
                ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        await _domainOfInfluenceAttachmentCountRepo.UpdateRequiredForVoterListsCount(doiId, isPoliticalAssembly: doi.Contest!.IsPoliticalAssembly);
        await _attachmentRepo.UpdateTotalCountsForDomainOfInfluence(doiId);
    }

    /// <summary>
    /// Gets all required attachments for each domain of influence in a contest (required != readable).
    /// An attachment is required if it gets delivered to the domain of influence.
    /// </summary>
    /// <param name="contestId">Contest id.</param>
    /// <returns>All required attachments by domain of influence id.</returns>
    internal async Task<Dictionary<Guid, List<Attachment>>> GetAllRequiredAttachmentsByDomainOfInfluenceId(Guid contestId)
    {
        var dois = await _domainOfInfluenceRepo.Query()
            .Where(x => x.ContestId == contestId)
            .Include(x => x.Attachments!)
            .ThenInclude(x => x.DomainOfInfluenceAttachmentCounts)
            .ToListAsync();

        var doiAttachmentCountsByDoiId = dois
            .SelectMany(doi => doi.Attachments!.SelectMany(a => a.DomainOfInfluenceAttachmentCounts!))
            .GroupBy(doiAc => doiAc.DomainOfInfluenceId, doiAc => doiAc)
            .ToDictionary(x => x.Key, x => x.ToList());

        var allRequiredAttachmentsByDoiId = new Dictionary<Guid, List<Attachment>>();
        foreach (var doi in dois)
        {
            allRequiredAttachmentsByDoiId[doi.Id] =
                doiAttachmentCountsByDoiId.TryGetValue(doi.Id, out var doiAttachmentCounts)
                    ? doiAttachmentCounts.ConvertAll(doiAc => doiAc.Attachment!)
                    : new();
        }

        return allRequiredAttachmentsByDoiId;
    }

    private async Task AddDomainOfInfluenceAttachmentCounts(Attachment attachment, int requiredCount)
    {
        var doiChildIds = (await _doiManager.ListChildren(attachment.DomainOfInfluenceId)).ConvertAll(x => x.Id);

        attachment.DomainOfInfluenceAttachmentCounts = doiChildIds.ConvertAll(doiId => new DomainOfInfluenceAttachmentCount
        {
            DomainOfInfluenceId = doiId,
        });

        attachment.DomainOfInfluenceAttachmentCounts.Add(new DomainOfInfluenceAttachmentCount
        {
            DomainOfInfluenceId = attachment.DomainOfInfluenceId,
            RequiredCount = requiredCount,
        });
    }

    private async Task SetDomainOfInfluenceAttachmentRequiredCount(Attachment attachment, Guid domainOfInfluenceId, int count)
    {
        var doiAttachmentCount = attachment.DomainOfInfluenceAttachmentCounts!.FirstOrDefault(x => x.DomainOfInfluenceId == domainOfInfluenceId)
            ?? throw new ForbiddenException($"The domain of influence {domainOfInfluenceId} has no permissions to set the required count on the attachment {attachment.Id}");

        doiAttachmentCount.RequiredCount = count;
        await _domainOfInfluenceAttachmentCountRepo.Update(doiAttachmentCount);
    }

    private async Task EnsureValidCreateOrUpdate(Attachment attachment, int requiredCount)
    {
        var pbIds = attachment.PoliticalBusinessEntries!.Select(x => x.PoliticalBusinessId).ToList();
        var doiId = attachment.DomainOfInfluenceId;
        var tenantId = _auth.Tenant.Id;

        var doi = await _domainOfInfluenceRepo.Query()
                      .WhereIsManager(tenantId)
                      .WhereContestNotLocked()
                      .WhereContestIsNotPastPrintingCenterSignUpDeadline(_clock)
                      .WhereCanHaveAttachments()
                      .Include(x => x.PoliticalBusinessPermissionEntries!)
                        .ThenInclude(x => x.PoliticalBusiness!.DomainOfInfluence)
                      .Include(x => x.Contest)
                      .FirstOrDefaultAsync(x => x.Id == doiId)
                  ?? throw new ForbiddenException("no permissions on contest or domain of influence or the domain of influence cannot have attachments");

        EnsureValidAttachmentCounts(attachment, doi.Type, requiredCount);

        var allowedPbIds = doi.PoliticalBusinessPermissionEntries!
            .Where(x => !x.PoliticalBusiness!.DomainOfInfluence!.ExternalPrintingCenter)
            .Select(x => x.PoliticalBusinessId)
            .ToList();

        if (pbIds.Any(pbId => !allowedPbIds.Contains(pbId)))
        {
            throw new ForbiddenException("only political businesses with permission entries allowed which are not on a domain of influence with external printing center");
        }

        if (!doi.Contest!.AttachmentDeliveryDeadline.HasValue || attachment.DeliveryPlannedOn.NextUtcDate(true) > doi.Contest!.AttachmentDeliveryDeadline.Value)
        {
            throw new ValidationException("the delivery must happen before or on the contest attachment delivery deadline");
        }
    }

    private async Task EnsureCanAssignPoliticalBusiness(Guid attachmentId, Guid politicalBusinessId)
    {
        if (!await _attachmentRepo.Query()
            .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
            .WhereContestIsNotLocked()
            .WhereContestIsNotPastPrintingCenterSignUpDeadline(_clock)
            .WhereDomainOfInfluenceHasPoliticalBusinessWithoutExternalPrintingCenter(politicalBusinessId)
            .AnyAsync(a => a.Id == attachmentId))
        {
            throw new ForbiddenException("no permissions on attachment, political business or contest");
        }
    }

    private async Task CreateCommentIfNeeded(
        Guid attachmentId,
        string commentContent)
    {
        if (string.IsNullOrWhiteSpace(commentContent))
        {
            return;
        }

        var comment = new AttachmentComment
        {
            Content = commentContent,
            CreatedAt = _clock.UtcNow,
            CreatedBy = await _userManager.GetCurrentUserOrEmpty(),
            AttachmentId = attachmentId,
        };
        await _attachmentCommentRepo.Create(comment);
    }

    private async Task EnsureAssignableDomainOfInfluenceIds(Attachment attachment, List<Guid> domainOfInfluenceIds)
    {
        if (domainOfInfluenceIds.Contains(attachment.DomainOfInfluenceId))
        {
            throw new ValidationException("You cannot set the domain of influence attachment count of the owner");
        }

        var validDoiIds = (await _doiManager.ListChildren(attachment.DomainOfInfluenceId)).ConvertAll(x => x.Id);
        if (domainOfInfluenceIds.Any(doiId => !validDoiIds.Contains(doiId)))
        {
            throw new ValidationException("Invalid domain of influence id found");
        }
    }

    private void EnsureValidAttachmentCounts(Attachment attachment, DomainOfInfluenceType domainOfInfluenceType, int requiredCount)
    {
        if (domainOfInfluenceType is DomainOfInfluenceType.Ch or DomainOfInfluenceType.Ct or DomainOfInfluenceType.Bz)
        {
            if (attachment.OrderedCount > 0 && requiredCount == 0)
            {
                return;
            }

            throw new ValidationException($"Attachments on domain of influence of type {domainOfInfluenceType} must have a ordered count greater than 0 and required count 0");
        }

        if (attachment.OrderedCount == requiredCount && requiredCount > 0)
        {
            return;
        }

        throw new ValidationException($"Attachments on domain of influence of type {domainOfInfluenceType} must have an equal ordered and required count and it has to be larger than 0");
    }
}
