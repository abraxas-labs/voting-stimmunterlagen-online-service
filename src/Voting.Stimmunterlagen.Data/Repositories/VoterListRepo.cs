// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Repositories;

public class VoterListRepo : DbRepository<DataContext, VoterList>
{
    public VoterListRepo(DataContext context)
        : base(context)
    {
    }

    public Task UpdateVotingCardCounts(Guid domainOfInfluenceId)
        => UpdateVotingCardCounts(new List<Guid> { domainOfInfluenceId });

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Referencing hardened inerpolated string parameters.")]
    public async Task UpdateVotingCardCounts(IReadOnlyCollection<Guid> domainOfInfluenceIds)
    {
        var domainOfInfluenceIdColName = GetDelimitedColumnName(vl => vl.DomainOfInfluenceId);
        var countOfVotingCardsColName = GetDelimitedColumnName(vl => vl.CountOfVotingCards);
        var countOfVotingCardsForHouseholdersColName = GetDelimitedColumnName(vl => vl.CountOfVotingCardsForHouseholders);
        var countOfVotingCardsForDomainOfInfluenceReturnAddressColName = GetDelimitedColumnName(vl => vl.CountOfVotingCardsForDomainOfInfluenceReturnAddress);
        var countOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAddressColName = GetDelimitedColumnName(vl => vl.CountOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAddress);

        var idColName = GetDelimitedColumnName(vl => vl.Id);

        var voterTableName = Context.Voters.GetDelimitedSchemaAndTableName();
        var voterListIdColName = Context.Voters.GetDelimitedColumnName(v => v.ListId);
        var voterVotingCardPrintDisabledColName = Context.Voters.GetDelimitedColumnName(v => v.VotingCardPrintDisabled);
        var voterIsHouseholderColName = Context.Voters.GetDelimitedColumnName(v => v.IsHouseholder);
        var voterSendVotingCardsToDomainOfInfluenceReturnAddressColName = Context.Voters.GetDelimitedColumnName(v => v.SendVotingCardsToDomainOfInfluenceReturnAddress);

        await Context.Database.ExecuteSqlRawAsync(
            $@"
                UPDATE {DelimitedSchemaAndTableName} VL
                SET {countOfVotingCardsColName} = (
                    SELECT COUNT(*)
                    FROM {voterTableName} V
                    WHERE V.{voterListIdColName} = VL.{idColName} AND V.{voterVotingCardPrintDisabledColName} = FALSE
                )
                WHERE VL.{domainOfInfluenceIdColName} = ANY({{0}})
            ",
            domainOfInfluenceIds);

        await Context.Database.ExecuteSqlRawAsync(
            $@"
                UPDATE {DelimitedSchemaAndTableName} VL
                SET {countOfVotingCardsForHouseholdersColName} = (
                    SELECT COUNT(*)
                    FROM {voterTableName} V
                    WHERE V.{voterListIdColName} = VL.{idColName}
                    AND V.{voterVotingCardPrintDisabledColName} = FALSE
                    AND V.{voterIsHouseholderColName} = TRUE
                )
                WHERE VL.{domainOfInfluenceIdColName} = ANY({{0}})
            ",
            domainOfInfluenceIds);

        await Context.Database.ExecuteSqlRawAsync(
            $@"
                UPDATE {DelimitedSchemaAndTableName} VL
                SET {countOfVotingCardsForDomainOfInfluenceReturnAddressColName} = (
                    SELECT COUNT(*)
                    FROM {voterTableName} V
                    WHERE V.{voterListIdColName} = VL.{idColName}
                    AND V.{voterVotingCardPrintDisabledColName} = FALSE
                    AND V.{voterSendVotingCardsToDomainOfInfluenceReturnAddressColName} = TRUE
                )
                WHERE VL.{domainOfInfluenceIdColName} = ANY({{0}})
            ",
            domainOfInfluenceIds);

        await Context.Database.ExecuteSqlRawAsync(
            $@"
                UPDATE {DelimitedSchemaAndTableName} VL
                SET {countOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAddressColName} = (
                    SELECT COUNT(*)
                    FROM {voterTableName} V
                    WHERE V.{voterListIdColName} = VL.{idColName}
                    AND V.{voterVotingCardPrintDisabledColName} = FALSE
                    AND V.{voterIsHouseholderColName} = TRUE
                    AND V.{voterSendVotingCardsToDomainOfInfluenceReturnAddressColName} = FALSE
                )
                WHERE VL.{domainOfInfluenceIdColName} = ANY({{0}})
            ",
            domainOfInfluenceIds);
    }
}
