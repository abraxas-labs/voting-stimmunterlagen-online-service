// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddDmDocCallbackInfoToJob : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CallbackToken",
            table: "VotingCardGeneratorJobs",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<int>(
            name: "DraftId",
            table: "VotingCardGeneratorJobs",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CallbackToken",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.DropColumn(
            name: "DraftId",
            table: "VotingCardGeneratorJobs");
    }
}
