// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Iam.Store;
using Voting.Stimmunterlagen.Core.Exceptions;
using Voting.Stimmunterlagen.Core.Managers.Steps;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Data.QueryableExtensions;
using Voting.Stimmunterlagen.Data.Repositories;
using Voting.Stimmunterlagen.Ech.Mapping;

namespace Voting.Stimmunterlagen.Core.Managers;

public class ManualVotingCardGeneratorJobManager
{
    private const string EchFillDefaultString = "?";

    private readonly IDbRepository<ManualVotingCardGeneratorJob> _jobsRepo;
    private readonly IDbRepository<DomainOfInfluenceVotingCardLayout> _layoutsRepo;
    private readonly TemplateManager _templateManager;
    private readonly UserManager _userManager;
    private readonly StepManager _stepManager;
    private readonly AttributeValidator _validator;
    private readonly IClock _clock;
    private readonly IAuth _auth;
    private readonly DomainOfInfluenceManager _doiManager;
    private readonly DataContext _dbContext;

    public ManualVotingCardGeneratorJobManager(
        IDbRepository<ManualVotingCardGeneratorJob> jobsRepo,
        TemplateManager templateManager,
        IDbRepository<DomainOfInfluenceVotingCardLayout> layoutsRepo,
        UserManager userManager,
        StepManager stepManager,
        IAuth auth,
        IClock clock,
        AttributeValidator validator,
        DomainOfInfluenceManager doiManager,
        DataContext dbContext)
    {
        _jobsRepo = jobsRepo;
        _templateManager = templateManager;
        _auth = auth;
        _layoutsRepo = layoutsRepo;
        _clock = clock;
        _validator = validator;
        _doiManager = doiManager;
        _dbContext = dbContext;
        _stepManager = stepManager;
        _userManager = userManager;
    }

    public Task<List<ManualVotingCardGeneratorJob>> List(Guid doiId)
    {
        return _jobsRepo.Query()
            .Include(x => x.Voter)
            .WhereIsOwner(_auth.Tenant.Id)
            .WhereHasDomainOfInfluence(doiId)
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }

    public async Task<Stream> CreateEmpty(Guid doiId, CancellationToken ct)
    {
        var layout = await GetLayout(doiId, VotingCardType.Swiss, ct);
        var voter = EmptyVoterBuilder.BuildEmptyVoter(layout.DomainOfInfluence!.Bfs);
        layout.DataConfiguration = new();

        if (!layout.DomainOfInfluence!.Contest!.IsPoliticalAssembly)
        {
            throw new ValidationException("Empty voting cards are only supported in political assemblies");
        }

        return await CreateInternal(doiId, voter, layout, ct);
    }

    public async Task<Stream> Create(Guid doiId, Voter voter, CancellationToken ct)
    {
        // only numbers are valid for "manual voting card generator job" and it should only contain digits
        if (voter.PersonId.Length > DatamatrixMapping.PersonIdLength || !voter.PersonId.All(char.IsDigit))
        {
            throw new ValidationException($"Invalid {nameof(voter.PersonId)} {voter.PersonId}");
        }

        var layout = await GetLayout(doiId, voter.VotingCardType, ct);

        voter.Bfs = "MANUAL";
        voter.ContestId = layout.DomainOfInfluence!.ContestId;
        FillVoterWithDefaultValuesForEchCompability(voter);

        voter.PersonId = DatamatrixMapping.MapPersonId(voter.PersonId);
        _validator.EnsureValid(voter);

        return await CreateInternal(doiId, voter, layout, ct);
    }

    internal async Task<Stream> CreateInternal(
        Guid doiId,
        Voter voter,
        DomainOfInfluenceVotingCardLayout layout,
        CancellationToken ct)
    {
        if (voter.VotingCardType != VotingCardType.Swiss)
        {
            throw new ValidationException("currently only swiss voting card types are supported");
        }

        var manualJob = new ManualVotingCardGeneratorJob
        {
            Created = _clock.UtcNow,
            CreatedBy = await _userManager.GetCurrentUserOrEmpty(),
            LayoutId = layout.Id,
            Voter = !string.IsNullOrEmpty(voter.PersonId) ? voter : null, // An empty voter is not persisted in the Database
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await _stepManager.EnsureStepApproved(doiId, Step.GenerateVotingCards);
        await _jobsRepo.Create(manualJob);
        var pdf = await _templateManager.GetPdf(layout.DomainOfInfluence!.Contest!.Date, layout, new[] { voter }, ct);
        await _doiManager.UpdateLastVoterUpdate(doiId);
        await transaction.CommitAsync();
        return pdf;
    }

    private async Task<DomainOfInfluenceVotingCardLayout> GetLayout(Guid doiId, VotingCardType votingCardType, CancellationToken ct)
    {
        return await _layoutsRepo.Query()
             .WhereIsDomainOfInfluenceManager(_auth.Tenant.Id)
             .WhereHasDomainOfInfluence(doiId)
             .WhereContestIsApproved()
             .WhereContestIsNotLocked()
             .Include(x => x.DomainOfInfluence!.Contest)
             .Include(x => x.TemplateDataFieldValues!).ThenInclude(x => x.Field!.Container)
             .FirstOrDefaultAsync(x => x.VotingCardType == votingCardType, ct)
             ?? throw new EntityNotFoundException(
                 nameof(DomainOfInfluenceVotingCardLayout),
                 new { votingCardType, doiId });
    }

    // fill default values which are necessary for eCH-0045 compability but are
    // not filled in the UI because they are only used for voting card generation
    // and not the accumulated eCH-0045 e-voting export.
    private void FillVoterWithDefaultValuesForEchCompability(Voter voter)
    {
        if (string.IsNullOrEmpty(voter.DateOfBirth))
        {
            voter.DateOfBirth = DatePartiallyKnownMapping.UnspecifiedDateString;
        }

        voter.PersonIdCategory = EchFillDefaultString;
        voter.MunicipalityName = EchFillDefaultString;
        voter.AddressLastName = EchFillDefaultString;
    }
}
