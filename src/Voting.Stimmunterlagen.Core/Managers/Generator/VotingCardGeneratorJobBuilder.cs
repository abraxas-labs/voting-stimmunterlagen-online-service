// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class VotingCardGeneratorJobBuilder
{
    private const string PdfFileExtension = ".pdf";

    private readonly ApiConfig _config;
    private readonly IDbRepository<VotingCardGeneratorJob> _jobsRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardLayout> _doiLayoutRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardConfiguration> _doiConfigRepo;
    private readonly IDbRepository<Voter> _voterRepo;

    public VotingCardGeneratorJobBuilder(
        ApiConfig config,
        IDbRepository<VotingCardGeneratorJob> jobsRepo,
        IDbRepository<DomainOfInfluenceVotingCardLayout> doiLayoutRepo,
        IDbRepository<DomainOfInfluenceVotingCardConfiguration> doiConfigRepo,
        IDbRepository<Voter> voterRepo)
    {
        _config = config;
        _jobsRepo = jobsRepo;
        _doiLayoutRepo = doiLayoutRepo;
        _doiConfigRepo = doiConfigRepo;
        _voterRepo = voterRepo;
    }

    internal static IEnumerable<List<Voter>> GroupVoters(IEnumerable<Voter> voters, IEnumerable<VotingCardGroup> groups)
        => voters.GroupBy(x => new VoterGroupKey(x, groups)).Select(x => x.ToList());

    internal async Task<List<VotingCardGeneratorJob>> CleanAndBuildJobs(Guid doiId, string tenantId, CancellationToken ct)
    {
        await CleanJobs(doiId, tenantId, ct);

        var config = await _doiConfigRepo.Query()
                         .Include(x => x.DomainOfInfluence)
                         .WhereIsDomainOfInfluenceManager(tenantId)
                         .FirstOrDefaultAsync(x => x.DomainOfInfluenceId == doiId, ct)
                     ?? throw new EntityNotFoundException(nameof(DomainOfInfluenceVotingCardConfiguration), doiId);
        var layouts = await _doiLayoutRepo.Query()
            .Include(x => x.DomainOfInfluence!.Contest)
            .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
            .WhereHasDomainOfInfluence(doiId)
            .WhereHasAccess(tenantId)
            .ToListAsync(ct);
        var layoutsByType = layouts.ToDictionary(x => x.VotingCardType);

        var voters = await _voterRepo.Query()
            .AsTracking()
            .Include(x => x.List)
            .WhereBelongToDomainOfInfluence(doiId)
            .Where(x => x.ListId.HasValue)
            .OrderBy(config.Sorts)
            .ToListAsync(ct);

        var jobs = GroupVoters(voters, config.Groups)
            .Select(g => BuildJob(g, layoutsByType, config.Groups, config.DomainOfInfluence!))
            .ToList();

        await _jobsRepo.CreateRange(jobs);
        return jobs;
    }

    private VotingCardGeneratorJob BuildJob(
        ICollection<Voter> voterGroup,
        IReadOnlyDictionary<VotingCardType, DomainOfInfluenceVotingCardLayout> layoutsByType,
        IEnumerable<VotingCardGroup> groups,
        ContestDomainOfInfluence doi)
    {
        var firstVoter = voterGroup.First();
        var votingCardType = firstVoter.List!.VotingCardType;

        DomainOfInfluenceVotingCardLayout? layout = null;

        if (!votingCardType.OfflineGenerationRequired() && (!layoutsByType.TryGetValue(firstVoter.List!.VotingCardType, out layout) || !layout.EffectiveTemplateId.HasValue))
        {
            throw new EntityNotFoundException(
                nameof(VotingCardLayout),
                new { firstVoter.List!.VotingCardType, firstVoter.List!.DomainOfInfluenceId });
        }

        return new VotingCardGeneratorJob
        {
            State = votingCardType.OfflineGenerationRequired()
                ? VotingCardGeneratorJobState.ReadyToRunOffline
                : VotingCardGeneratorJobState.Ready,
            Voter = voterGroup,
            FileName = BuildFileName(doi, groups, firstVoter),
            CountOfVoters = voterGroup.Count,
            LayoutId = layout?.Id,
            DomainOfInfluenceId = doi.Id,
        };
    }

    private async Task CleanJobs(Guid doiId, string tenantId, CancellationToken ct)
    {
        var toDelete = await _jobsRepo.Query()
            .Where(x => x.DomainOfInfluenceId == doiId && x.DomainOfInfluence!.SecureConnectId == tenantId)
            .Select(x => x.Id)
            .ToListAsync(ct);
        await _jobsRepo.DeleteRangeByKey(toDelete);
    }

    private string BuildFileName(ContestDomainOfInfluence doi, IEnumerable<VotingCardGroup> groups, Voter voter)
    {
        var doiNameWithoutSpace = doi.Name.Replace(" ", "_");
        var fileNamePrefix = !string.IsNullOrWhiteSpace(doi.Bfs)
            ? doi.Bfs + _config.VotingCardGenerator.FileNameGroupSeparator + doiNameWithoutSpace
            : doiNameWithoutSpace;

        var groupsName = string.Join(
                _config.VotingCardGenerator.FileNameGroupSeparator,
                groups.Select(voter.GetGroupValue).Where(x => x != null));

        var name = fileNamePrefix + _config.VotingCardGenerator.FileNameGroupSeparator + groupsName;
        return FileNameUtils.SanitizeFileName(name) + PdfFileExtension;
    }
}
