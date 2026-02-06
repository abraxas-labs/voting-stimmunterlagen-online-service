// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddEmptyVotingCardsCount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasEmptyVotingCards",
            table: "VotingCardGeneratorJobs",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "CountOfEmptyVotingCards",
            table: "ContestDomainOfInfluences",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastCountOfEmptyVotingCardsUpdate",
            table: "ContestDomainOfInfluences",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HasEmptyVotingCards",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.DropColumn(
            name: "CountOfEmptyVotingCards",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "LastCountOfEmptyVotingCardsUpdate",
            table: "ContestDomainOfInfluences");
    }
}
