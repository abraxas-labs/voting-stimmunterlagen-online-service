// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingCardGeneratorJobsAndConfigurations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Bfs",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<Guid>(
            name: "JobId",
            table: "Voters",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LanguageOfCorrespondence",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<int>(
            name: "SourceIndex",
            table: "Voters",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "GenerateVotingCardsTriggered",
            table: "ContestDomainOfInfluences",
            type: "timestamp without time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceVotingCardConfigurations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                SampleCount = table.Column<int>(type: "integer", nullable: false),
                Groups = table.Column<int[]>(type: "integer[]", nullable: false),
                Sorts = table.Column<int[]>(type: "integer[]", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceVotingCardConfigurations", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceVotingCardConfigurations_ContestDomainOfIn~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VotingCardGeneratorJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false),
                CountOfVoters = table.Column<int>(type: "integer", nullable: false),
                Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Completed = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                Failed = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                State = table.Column<int>(type: "integer", nullable: false),
                LayoutId = table.Column<Guid>(type: "uuid", nullable: false),
                Runner = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VotingCardGeneratorJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_VotingCardGeneratorJobs_DomainOfInfluenceVotingCardLayouts_~",
                    column: x => x.LayoutId,
                    principalTable: "DomainOfInfluenceVotingCardLayouts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Voters_JobId",
            table: "Voters",
            column: "JobId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVotingCardConfigurations_DomainOfInfluence~",
            table: "DomainOfInfluenceVotingCardConfigurations",
            column: "DomainOfInfluenceId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_VotingCardGeneratorJobs_LayoutId",
            table: "VotingCardGeneratorJobs",
            column: "LayoutId");

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_VotingCardGeneratorJobs_JobId",
            table: "Voters",
            column: "JobId",
            principalTable: "VotingCardGeneratorJobs",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Voters_VotingCardGeneratorJobs_JobId",
            table: "Voters");

        migrationBuilder.DropTable(
            name: "DomainOfInfluenceVotingCardConfigurations");

        migrationBuilder.DropTable(
            name: "VotingCardGeneratorJobs");

        migrationBuilder.DropIndex(
            name: "IX_Voters_JobId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "Bfs",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "JobId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "LanguageOfCorrespondence",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SourceIndex",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "GenerateVotingCardsTriggered",
            table: "ContestDomainOfInfluences");
    }
}
