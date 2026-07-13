// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class RemoveVotingCardLayoutColor : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DomainOfInfluenceVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "OverriddenVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "ContestVotingCardLayouts");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "DomainOfInfluenceVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "OverriddenVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
