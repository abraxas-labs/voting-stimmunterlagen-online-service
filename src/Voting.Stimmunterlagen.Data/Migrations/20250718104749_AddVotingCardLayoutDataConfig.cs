// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVotingCardLayoutDataConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDateOfBirth",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeIsHouseholder",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludePersonId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeReligion",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDateOfBirth",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeIsHouseholder",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludePersonId",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeReligion",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDateOfBirth",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeIsHouseholder",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludePersonId",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeReligion",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDateOfBirth",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeIsHouseholder",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludePersonId",
            table: "ContestVotingCardLayouts");
    }
}
