// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers;

public class PoliticalBusinessManager
{
    private readonly IAuth _auth;
    private readonly IDbRepository<PoliticalBusiness> _politicalBusinessRepo;
    private readonly IDbRepository<Attachment> _attachmentRepo;
    private readonly IDbRepository<ContestDomainOfInfluence> _domainOfInfluenceRepo;

    public PoliticalBusinessManager(
        IAuth auth,
        IDbRepository<PoliticalBusiness> politicalBusinessRepo,
        IDbRepository<Attachment> attachmentRepo,
        IDbRepository<ContestDomainOfInfluence> domainOfInfluenceRepo)
    {
        _auth = auth;
        _politicalBusinessRepo = politicalBusinessRepo;
        _attachmentRepo = attachmentRepo;
        _domainOfInfluenceRepo = domainOfInfluenceRepo;
    }

    public Task<List<PoliticalBusiness>> List(Guid? contestId, Guid? domainOfInfluenceId)
    {
        var tenantId = _auth.Tenant.Id;
        return _politicalBusinessRepo.Query()
            .Include(x => x.DomainOfInfluence)
            .Include(x => x.Translations)
            .Where(x => (contestId == null || x.ContestId == contestId)
                && x.PermissionEntries!.Any(p => p.SecureConnectId == tenantId
                    && (domainOfInfluenceId == null || p.DomainOfInfluenceId == domainOfInfluenceId)))
            .OrderBy(x => x.DomainOfInfluence!.Type)
            .ThenBy(x => x.PoliticalBusinessNumber)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<PoliticalBusiness>> ListAttachmentAccessible(Guid domainOfInfluenceId, bool forPrintJobManager)
    {
        var tenantId = _auth.Tenant.Id;

        var doi = await _domainOfInfluenceRepo.Query()
            .Include(doi => doi.PoliticalBusinesses!)
            .Include(doi => doi.Contest!.DomainOfInfluence!)
            .Include(doi => doi.PoliticalBusinessPermissionEntries!)
            .ThenInclude(x => x.PoliticalBusiness!.DomainOfInfluence)
            .Include(doi => doi.PoliticalBusinessPermissionEntries!)
            .ThenInclude(x => x.PoliticalBusiness!.Translations)
            .FirstOrDefaultAsync(doi => doi.Id == domainOfInfluenceId);

        if (doi == null)
        {
            return new();
        }

        if (forPrintJobManager)
        {
            return ListAttachmentAccessible(doi);
        }

        var isContestManager = doi.Contest!.DomainOfInfluence!.SecureConnectId == tenantId;
        var canReadAttachmentsQuery = _attachmentRepo.Query();

        canReadAttachmentsQuery = isContestManager
            ? canReadAttachmentsQuery.WhereIsContestManager(tenantId)
            : canReadAttachmentsQuery.WhereHasAccess(tenantId, doi.ContestId);

        var canReadAttachments = await canReadAttachmentsQuery
            .Select(a => a.DomainOfInfluenceId)
            .AnyAsync(d => d == domainOfInfluenceId);

        if (!canReadAttachments)
        {
            return new();
        }

        return ListAttachmentAccessible(doi);
    }

    internal List<PoliticalBusiness> ListAttachmentAccessible(ContestDomainOfInfluence doi)
    {
        return doi.PoliticalBusinessPermissionEntries!
            .Where(x => !x.PoliticalBusiness!.DomainOfInfluence!.ExternalPrintingCenter)
            .Select(x => x.PoliticalBusiness!)
            .OrderBy(pb => pb.PoliticalBusinessNumber)
            .DistinctBy(pb => pb.Id)
            .ToList();
    }
}
