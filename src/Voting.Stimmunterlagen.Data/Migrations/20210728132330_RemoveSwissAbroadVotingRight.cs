// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RemoveSwissAbroadVotingRight : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SwissAbroadVotingRight",
            table: "PoliticalBusinesses");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "SwissAbroadVotingRight",
            table: "PoliticalBusinesses",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
