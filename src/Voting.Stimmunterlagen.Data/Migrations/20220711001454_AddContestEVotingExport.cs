// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddContestEVotingExport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CantonSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Canton = table.Column<int>(type: "integer", nullable: false),
                VotingDocumentsEVotingEaiMessageType = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CantonSettings", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContestEVotingExportJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false),
                Started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Failed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                State = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestEVotingExportJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestEVotingExportJobs_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CantonSettings_Canton",
            table: "CantonSettings",
            column: "Canton",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContestEVotingExportJobs_ContestId",
            table: "ContestEVotingExportJobs",
            column: "ContestId",
            unique: true);

        migrationBuilder.Sql(@$"
            INSERT INTO ""ContestEVotingExportJobs"" (""Id"", ""ContestId"", ""FileName"", ""State"")
            SELECT uuid_generate_v4(), ""Id"", 'Contest_EVoting.zip', 1
                FROM ""Contests""
                WHERE ""EVoting"" = true");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CantonSettings");

        migrationBuilder.DropTable(
            name: "ContestEVotingExportJobs");
    }
}
