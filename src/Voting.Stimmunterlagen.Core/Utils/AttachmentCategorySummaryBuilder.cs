// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Utils;

public class AttachmentCategorySummaryBuilder
{
    private readonly IDbRepository<VoterList> _voterListRepo;
    private readonly ContestRepo _contestRepo;
    private readonly ContestDomainOfInfluenceRepo _domainOfInfluenceRepo;

    public AttachmentCategorySummaryBuilder(
        IDbRepository<VoterList> voterListRepo,
        ContestRepo contestRepo,
        ContestDomainOfInfluenceRepo domainOfInfluenceRepo)
    {
        _voterListRepo = voterListRepo;
        _contestRepo = contestRepo;
        _domainOfInfluenceRepo = domainOfInfluenceRepo;
    }

    public async Task<Dictionary<Guid, List<AttachmentCategorySummary>>> BuildGroupedByDomainOfInfluence(List<Attachment> attachments, Guid domainOfInfluenceId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluenceId == domainOfInfluenceId);
        var doi = await LoadDomainOfInfluence(domainOfInfluenceId);
        return attachments.GroupBy(a => a.DomainOfInfluenceId).ToDictionary(x => x.Key, x => Build(x.ToList(), voterLists, doi.Contest!.IsPoliticalAssembly));
    }

    public async Task<List<AttachmentCategorySummary>> BuildForDomainOfInfluence(List<Attachment> attachments, Guid domainOfInfluenceId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluenceId == domainOfInfluenceId);
        var doi = await LoadDomainOfInfluence(domainOfInfluenceId);
        return Build(attachments, voterLists, doi.Contest!.IsPoliticalAssembly);
    }

    public async Task<List<AttachmentCategorySummary>> BuildForContest(List<Attachment> attachments, Guid contestId)
    {
        var voterLists = await LoadVoterLists(vl => vl.DomainOfInfluence!.ContestId == contestId);

        var contest = await _contestRepo.GetByKey(contestId)
            ?? throw new EntityNotFoundException(nameof(Contest), contestId);

        return Build(attachments, voterLists, contest.IsPoliticalAssembly);
    }

    internal List<AttachmentCategorySummary> Build(List<Attachment> attachments, List<VoterList> voterLists, bool isPoliticalAssembly)
    {
        var attachmentsByCategory = attachments
            .GroupBy(a => a.Category)
            .ToDictionary(x => x.Key, x => x.ToList());

        return attachmentsByCategory
            .Select(attachmentsByCategoryEntry =>
            {
                var categoryAttachments = attachmentsByCategoryEntry.Value;
                var sendAllAttachmentsOnlyToHouseholder = categoryAttachments.All(a => a.SendOnlyToHouseholder);

                var attachmentPbIds = categoryAttachments
                    .SelectMany(a => a.PoliticalBusinessEntries)
                    .Select(e => e.PoliticalBusinessId)
                    .ToHashSet();

                var requiredForVoterListsCount = voterLists
                    .Where(v => v.PoliticalBusinessEntries!.Any(e => attachmentPbIds.Contains(e.PoliticalBusinessId)) || isPoliticalAssembly)
                    .Sum(v => sendAllAttachmentsOnlyToHouseholder ? v.CountOfVotingCardsForHouseholders : v.CountOfVotingCards);

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

    private async Task<ContestDomainOfInfluence> LoadDomainOfInfluence(Guid doiId)
    {
        return await _domainOfInfluenceRepo
            .Query()
            .Include(x => x.Contest)
            .FirstOrDefaultAsync(x => x.Id == doiId)
            ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
    }
}
