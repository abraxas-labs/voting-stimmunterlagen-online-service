// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVotingCardCounts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "NumberOfVotingCards",
            table: "VoterLists",
            newName: "CountOfVotingCards");

        migrationBuilder.RenameColumn(
            name: "NumberOfHouseholders",
            table: "VoterLists",
            newName: "CountOfVotingCardsForHouseholders");

        migrationBuilder.RenameColumn(
            name: "CountOfSendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "VoterLists",
            newName: "CountOfVotingCardsForDomainOfInfluenceReturnAddress");

        migrationBuilder.AddColumn<int>(
            name: "CountOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAd~",
            table: "VoterLists",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CountOfVotingCardsForHouseholdersExclDomainOfInfluenceReturnAd~",
            table: "VoterLists");

        migrationBuilder.RenameColumn(
            name: "CountOfVotingCards",
            table: "VoterLists",
            newName: "NumberOfVotingCards");

        migrationBuilder.RenameColumn(
            name: "CountOfVotingCardsForHouseholders",
            table: "VoterLists",
            newName: "NumberOfHouseholders");

        migrationBuilder.RenameColumn(
            name: "CountOfVotingCardsForDomainOfInfluenceReturnAddress",
            table: "VoterLists",
            newName: "CountOfSendVotingCardsToDomainOfInfluenceReturnAddress");
    }
}
