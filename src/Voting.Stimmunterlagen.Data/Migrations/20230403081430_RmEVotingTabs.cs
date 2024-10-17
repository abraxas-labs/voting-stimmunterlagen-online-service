// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RmEVotingTabs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_VotingCardGeneratorJobs_DomainOfInfluenceVotingCardLayouts_~",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.AddForeignKey(
            name: "FK_VotingCardGeneratorJobs_DomainOfInfluenceVotingCardLayouts_~",
            table: "VotingCardGeneratorJobs",
            column: "LayoutId",
            principalTable: "DomainOfInfluenceVotingCardLayouts",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AlterColumn<Guid>(
            name: "LayoutId",
            table: "VotingCardGeneratorJobs",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddColumn<Guid>(
            name: "DomainOfInfluenceId",
            table: "VotingCardGeneratorJobs",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.Sql(@"
                UPDATE ""VotingCardGeneratorJobs"" vc
                SET ""DomainOfInfluenceId"" = l.""DomainOfInfluenceId""
                FROM ""DomainOfInfluenceVotingCardLayouts"" l
                WHERE vc.""LayoutId"" = l.""Id""");

        migrationBuilder.Sql(@"DELETE FROM ""ContestVotingCardLayouts"" WHERE ""VotingCardType"" = 3");
        migrationBuilder.Sql(@"DELETE FROM ""DomainOfInfluenceVotingCardLayouts"" WHERE ""VotingCardType"" = 3");

        migrationBuilder.CreateIndex(
            name: "IX_VotingCardGeneratorJobs_DomainOfInfluenceId",
            table: "VotingCardGeneratorJobs",
            column: "DomainOfInfluenceId");

        migrationBuilder.AddForeignKey(
            name: "FK_VotingCardGeneratorJobs_ContestDomainOfInfluences_DomainOfI~",
            table: "VotingCardGeneratorJobs",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_VotingCardGeneratorJobs_ContestDomainOfInfluences_DomainOfI~",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.DropIndex(
            name: "IX_VotingCardGeneratorJobs_DomainOfInfluenceId",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.DropColumn(
            name: "DomainOfInfluenceId",
            table: "VotingCardGeneratorJobs");

        migrationBuilder.AlterColumn<Guid>(
            name: "LayoutId",
            table: "VotingCardGeneratorJobs",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);
    }
}
