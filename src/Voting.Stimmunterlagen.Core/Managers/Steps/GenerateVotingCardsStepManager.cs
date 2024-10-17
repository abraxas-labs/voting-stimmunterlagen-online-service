// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Data.Models;
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

    public GenerateVotingCardsStepManager(
        VotingCardGeneratorJobBuilder jobBuilder,
        VotingCardGeneratorLauncher jobLauncher,
        IDbRepository<ContestDomainOfInfluence> doiRepo,
        IClock clock,
        PrintJobBuilder printJobBuilder,
        AttachmentManager attachmentManager)
    {
        _jobBuilder = jobBuilder;
        _jobLauncher = jobLauncher;
        _doiRepo = doiRepo;
        _clock = clock;
        _printJobBuilder = printJobBuilder;
        _attachmentManager = attachmentManager;
    }

    public override Step Step => Step.GenerateVotingCards;

    public override async Task Approve(Guid domainOfInfluenceId, string tenantId, CancellationToken ct)
    {
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
}
