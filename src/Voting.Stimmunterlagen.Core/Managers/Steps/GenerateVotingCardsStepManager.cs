// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Voting.Stimmunterlagen.Core.Managers.Steps;

public class GenerateVotingCardsStepManager : NonRevertableStepManager
{
    private readonly VotingCardGeneratorJobBuilder _jobBuilder;
    private readonly VotingCardGeneratorLauncher _jobLauncher;
    private readonly IDbRepository<ContestDomainOfInfluence> _doiRepo;
    private readonly IClock _clock;
    private readonly PrintJobBuilder _printJobBuilder;
    private readonly AttachmentManager _attachmentManager;
    private readonly VoterListRepo _voterListRepo;
    private readonly PoliticalBusinessManager _pbManager;

    public GenerateVotingCardsStepManager(
        VotingCardGeneratorJobBuilder jobBuilder,
        VotingCardGeneratorLauncher jobLauncher,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IClock clock,
        PrintJobBuilder printJobBuilder,
        AttachmentManager attachmentManager,
        VoterListRepo voterListRepo,
        PoliticalBusinessManager pbManager)
    {
        _jobBuilder = jobBuilder;
        _jobLauncher = jobLauncher;
        _doiRepo = doiRepo;
        _clock = clock;
        _printJobBuilder = printJobBuilder;
        _attachmentManager = attachmentManager;
        _voterListRepo = voterListRepo;
        _pbManager = pbManager;
    }

    public override Step Step => Step.GenerateVotingCards;

    public override async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
        var voterListsCount = await GetVoterListsCount(domainOfInfluenceId, tenantId);
        if (voterListsCount == 0)
        {
            throw new ValidationException("Cannot approve generate voting card steps if no voter list is imported");
        }

        var doi = await _doiRepo.Query()
            .WhereIsManager(tenantId)
            .Include(doi => doi.CountingCircles!)
            .ThenInclude(doiCc => doiCc.CountingCircle)
            .Include(doi => doi.Contest)
            .FirstOrDefaultAsync(doi => doi.Id == domainOfInfluenceId)
            ?? throw new EntityNotFoundException(nameof(Contest), domainOfInfluenceId);

        if (doi.Contest!.EVoting && doi.CountingCircles!.Any(doiCc => doiCc.CountingCircle!.EVoting))
        {
            await EnsurePoliticalBusinessEVotingApproved(domainOfInfluenceId);
        }

        var jobs = await _jobBuilder.CleanAndBuildJobs(domainOfInfluenceId, tenantId, ct);
        await SetVotingCardGenerationTriggered(domainOfInfluenceId);

        var readyToRunJobIds = jobs
            .Where(x => x.State == VotingCardGeneratorJobState.Ready)
            .Select(x => x.Id);
        _ = _jobLauncher.RunJobs(readyToRunJobIds);
    }

    private async Task SetVotingCardGenerationTriggered(Guid doiId)
    {
        var doi = await _doiRepo.GetByKey(doiId)
                  ?? throw new EntityNotFoundException(nameof(ContestDomainOfInfluence), doiId);
        doi.GenerateVotingCardsTriggered = _clock.UtcNow;
        await _doiRepo.UpdateIgnoreRelations(doi);
        await _printJobBuilder.SyncStateForDomainOfInfluence(doiId, await _attachmentManager.GetAllRequiredAttachmentsByDomainOfInfluenceId(doi.ContestId));
    }

    private Task<int> GetVoterListsCount(Guid doiId, string tenantId)
    {
        return _voterListRepo.Query()
            .WhereIsDomainOfInfluenceManager(tenantId)
            .CountAsync(vl => vl.DomainOfInfluenceId == doiId);
    }

    private async Task EnsurePoliticalBusinessEVotingApproved(Guid domainOfInfluenceId)
    {
        var accessiblePbs = await _pbManager.List(null, domainOfInfluenceId);
        var pbWithEVotingNotApproved = accessiblePbs.Find(pb => pb.EVotingApproved == false);

        if (pbWithEVotingNotApproved != null)
        {
            throw new ValidationException($"Political business {pbWithEVotingNotApproved.Id} has not approved e-voting");
        }
    }
}
