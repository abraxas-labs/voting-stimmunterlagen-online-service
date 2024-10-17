// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddContestState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Contests_TestingPhaseEnded",
            table: "Contests");

        migrationBuilder.AddColumn<int>(
            name: "State",
            table: "Contests",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Contests_State",
            table: "Contests",
            column: "State");

        // set states, past state will be set automatically as soon as the job generates the events
        migrationBuilder.Sql("UPDATE \"Contests\" SET \"State\" = 1");
        migrationBuilder.Sql("UPDATE \"Contests\" SET \"State\" = 2 WHERE \"TestingPhaseEnded\"");

        migrationBuilder.DropColumn(
            name: "TestingPhaseEnded",
            table: "Contests");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Contests_State",
            table: "Contests");

        migrationBuilder.DropColumn(
            name: "State",
            table: "Contests");

        migrationBuilder.AddColumn<bool>(
            name: "TestingPhaseEnded",
            table: "Contests",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_Contests_TestingPhaseEnded",
            table: "Contests",
            column: "TestingPhaseEnded");
    }
}
