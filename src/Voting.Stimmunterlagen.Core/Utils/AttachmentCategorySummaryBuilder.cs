// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Utils;

public class AttachmentCategorySummaryBuilder
{
    private readonly IDbRepository<VoterList> _voterListRepo;

    public AttachmentCategorySummaryBuilder(IDbRepository<VoterList> voterListRepo)
    {
        _voterListRepo = voterListRepo;
    }

    public async Task<Dictionary<Guid, List<AttachmentCategorySummary>>> BuildGroupedByDomainOfInfluence(List<Attachment> attachments, Guid domainOfInfluenceId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluenceId == domainOfInfluenceId);
        return attachments.GroupBy(a => a.DomainOfInfluenceId).ToDictionary(x => x.Key, x => Build(x.ToList(), voterLists));
    }

    public async Task<List<AttachmentCategorySummary>> BuildForDomainOfInfluence(List<Attachment> attachments, Guid domainOfInfluenceId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluenceId == domainOfInfluenceId);
        return Build(attachments, voterLists);
    }

    public async Task<List<AttachmentCategorySummary>> BuildForContest(List<Attachment> attachments, Guid contestId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluence!.ContestId == contestId);
        return Build(attachments, voterLists);
    }

    internal List<AttachmentCategorySummary> Build(List<Attachment> attachments, List<VoterList> voterLists)
    {
        var isPoliticalAssembly = voterLists.FirstOrDefault()?.DomainOfInfluence?.Contest?.IsPoliticalAssembly == true;

        var attachmentsByCategory = attachments
            .GroupBy(a => a.Category)
            .ToDictionary(x => x.Key, x => x.ToList());

        return attachmentsByCategory
            .Select(attachmentsByCategoryEntry =>
            {
                var attachmentPbIds = attachmentsByCategoryEntry
                    .Value
                    .SelectMany(a => a.PoliticalBusinessEntries)
                    .Select(e => e.PoliticalBusinessId)
                    .ToHashSet();

                var requiredForVoterListsCount = voterLists
                    .Where(v => v.PoliticalBusinessEntries!.Any(e => attachmentPbIds.Contains(e.PoliticalBusinessId)) || isPoliticalAssembly)
                    .Sum(v => v.NumberOfVoters);

                return new AttachmentCategorySummary(
                    attachmentsByCategoryEntry.Key,
                    attachmentsByCategoryEntry.Value,
                    requiredForVoterListsCount);
            })
            .OrderBy(a => a.Category)
            .ToList();
    }

    private async Task<List<VoterList>> LoadVoterLists(Expression<Func<VoterList, bool>> predicate)
    {
        return await _voterListRepo.Query()
            .Include(vl => vl.PoliticalBusinessEntries)
            .Include(vl => vl.DomainOfInfluence!.Contest)
            .Where(predicate)
            .ToListAsync();
    }
}
