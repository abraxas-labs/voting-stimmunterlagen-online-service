// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public class VoterListImportBatchHandler
{
    private readonly DbContextOptions _dbContextOptions;
    private readonly DataContext _dbContext;

    public VoterListImportBatchHandler(
        DbContextOptions dbContextOptions,
        DataContext dbContext)
    {
        _dbContextOptions = dbContextOptions;
        _dbContext = dbContext;
    }

    public async Task CreateVoters(
        Voter[] voterChunk,
        Dictionary<VotingCardType, VoterList> listByVcType,
        Guid contestId,
        DateTime contestDate,
        bool ignoreSendVotingCardsToDoiReturnAddress,
        bool electoralRegisterMultipleEnabled,
        VoterDuplicatesBuilder voterDuplicatesBuilder,
        VoterHouseholdBuilder voterHouseholdBuilder,
        HashSet<VoterDuplicateKey> errorDuplicates,
        CancellationToken ct)
    {
        var votersToCreate = new List<Voter>();
        var voterDuplicatesToCreate = new List<VoterDuplicatesBuilderVoterDuplicateData>();

        foreach (var voter in voterChunk)
        {
            var list = listByVcType[voter.VotingCardType];
            voter.ListId = list.Id;
            voter.ContestId = contestId;
            list.NumberOfVoters++;

            if (voter.SendVotingCardsToDomainOfInfluenceReturnAddress && ignoreSendVotingCardsToDoiReturnAddress)
            {
                voter.SendVotingCardsToDomainOfInfluenceReturnAddress = false;
            }

            var voterDuplicatesNextVoterResult = voterDuplicatesBuilder.NextVoter(voter);
            voterHouseholdBuilder.NextVoter(voter);

            HandleVoterDuplicatesCheckResult(
                voterDuplicatesNextVoterResult,
                voter,
                votersToCreate,
                errorDuplicates,
                voterDuplicatesToCreate,
                electoralRegisterMultipleEnabled);

            voter.IsMinor = DatamatrixMapping.IsMinor(voter.DateOfBirth, contestDate);
        }

        if (votersToCreate.Count == 0)
        {
            return;
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

        await CreateNewVoterDuplicates(dbContext, voterDuplicatesToCreate, votersToCreate, ct);

        dbContext.Voters.AddRange(votersToCreate);
        await dbContext.SaveChangesAsync(ct);

        await UpdateVoterDuplicatesPrintDisabled(dbContext, votersToCreate, ct);
    }

    private void HandleVoterDuplicatesCheckResult(
        VoterDuplicatesBuilderNextVoterResult duplicateCheckResult,
        Voter voter,
        List<Voter> votersToCreate,
        HashSet<VoterDuplicateKey> errorDuplicates,
        List<VoterDuplicatesBuilderVoterDuplicateData> voterDuplicatesToCreate,
        bool electoralRegisterMultipleEnabled)
    {
        if (duplicateCheckResult.State is VoterDuplicatesBuilderNextVoterResultState.NoActionRequired)
        {
            votersToCreate.Add(voter);
            return;
        }

        if (duplicateCheckResult.State is VoterDuplicatesBuilderNextVoterResultState.InternalDuplicate)
        {
            errorDuplicates.Add(new VoterDuplicateKey(voter.FirstName, voter.LastName, voter.DateOfBirth, voter.Street, voter.HouseNumber, false));
            return;
        }

        if (electoralRegisterMultipleEnabled)
        {
            if (duplicateCheckResult.State is VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateCreateRequired)
            {
                voter.VoterDuplicate = duplicateCheckResult.Data!.VoterDuplicate;
                voterDuplicatesToCreate.Add(duplicateCheckResult.Data);
                votersToCreate.Add(voter);
            }
            else if (duplicateCheckResult.State is VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateReferenceRequired)
            {
                voter.VoterDuplicateId = duplicateCheckResult.Data!.VoterDuplicate.Id;
                votersToCreate.Add(voter);
            }

            return;
        }

        // If electoral register multiple is disabled, no external duplicates are allowed.
        if (duplicateCheckResult.State is VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateCreateRequired or VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateReferenceRequired)
        {
            errorDuplicates.Add(new VoterDuplicateKey(voter.FirstName, voter.LastName, voter.DateOfBirth, voter.Street, voter.HouseNumber, true));
        }
    }

    private async Task CreateNewVoterDuplicates(
        DataContext dbContext,
        List<VoterDuplicatesBuilderVoterDuplicateData> voterDuplicatesDataToCreate,
        List<Voter> votersToCreate,
        CancellationToken ct)
    {
        var voterDuplicatesToCreate = voterDuplicatesDataToCreate.ConvertAll(c => c.VoterDuplicate);

        if (voterDuplicatesDataToCreate.Count == 0)
        {
            return;
        }

        dbContext.DomainOfInfluenceVoterDuplicates.AddRange(voterDuplicatesToCreate);
        await dbContext.SaveChangesAsync(ct);

        // New voter duplicates are created and have an id, to simplify the workflow, we want to handle the
        // whole "UpdateVoterDuplicatesPrintDisabled" process uniform.
        // This is achieved by only handling the workflow where the duplicate entities already exist.
        foreach (var voter in votersToCreate)
        {
            if (voter.VoterDuplicate == null)
            {
                continue;
            }

            voter.VoterDuplicateId = voter.VoterDuplicate.Id;
            voter.VoterDuplicate = null;
        }

        var externalVoterIds = voterDuplicatesDataToCreate.SelectMany(x => x.ExistingExternalVoterIds).ToList();
        var externalVotersToUpdate = await dbContext.Voters.Where(v => externalVoterIds.Contains(v.Id)).ToListAsync(ct);

        if (externalVotersToUpdate.Count != externalVoterIds.Count)
        {
            throw new ValidationException($"Count mismatch, external voters to update: {externalVotersToUpdate.Count}, external voter ids: {externalVoterIds.Count}");
        }

        foreach (var voterDuplicatesData in voterDuplicatesDataToCreate)
        {
            foreach (var voter in externalVotersToUpdate.Where(v => voterDuplicatesData.ExistingExternalVoterIds.Contains(v.Id)))
            {
                voter.VoterDuplicateId = voterDuplicatesData.VoterDuplicate.Id;
                voter.VotingCardPrintDisabled = true;
            }
        }

        dbContext.Voters.UpdateRange(externalVotersToUpdate);
        await dbContext.SaveChangesAsync();
    }

    private async Task UpdateVoterDuplicatesPrintDisabled(DataContext dbContext, List<Voter> createdVoters, CancellationToken ct)
    {
        var newVoterIds = createdVoters.ConvertAll(v => v.Id);
        var voterDuplicatesId = createdVoters.Where(v => v.VoterDuplicateId != null).Select(v => v.VoterDuplicateId!.Value).ToList();

        if (voterDuplicatesId.Count == 0)
        {
            return;
        }

        var voterDuplicates = await dbContext.DomainOfInfluenceVoterDuplicates
            .Where(d => voterDuplicatesId.Contains(d.Id))
            .Include(d => d.Voters)
            .ToListAsync();

        var votersToDisablePrintIds = voterDuplicates
            .SelectMany(d => d.Voters!)
            .Where(v => !newVoterIds.Contains(v.Id))
            .Select(v => v.Id)
            .ToList();

        await dbContext.Voters
            .Where(v => votersToDisablePrintIds.Contains(v.Id))
            .ExecuteUpdateAsync(v => v.SetProperty(e => e.VotingCardPrintDisabled, true), cancellationToken: ct);

        await dbContext.SaveChangesAsync(ct);
    }
}
