// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class ModifyContestDeleteBehavior : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests");

        migrationBuilder.AddForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests");

        migrationBuilder.AddForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id");
    }
}
