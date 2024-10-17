// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddPrintJob : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PrintJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                State = table.Column<int>(type: "integer", nullable: false),
                ProcessStartedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ProcessEndedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                DoneOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                VotingCardsPrintedCount = table.Column<int>(type: "integer", nullable: false),
                VotingCardsPackedCount = table.Column<int>(type: "integer", nullable: false),
                VotingCardsSentCount = table.Column<int>(type: "integer", nullable: false),
                DoneComment = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PrintJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_PrintJobs_ContestDomainOfInfluences_DomainOfInfluenceId",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluences_Name",
            table: "ContestDomainOfInfluences",
            column: "Name")
            .Annotation("Npgsql:IndexMethod", "gin")
            .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

        migrationBuilder.CreateIndex(
            name: "IX_PrintJobs_DomainOfInfluenceId",
            table: "PrintJobs",
            column: "DomainOfInfluenceId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PrintJobs");

        migrationBuilder.DropIndex(
            name: "IX_ContestDomainOfInfluences_Name",
            table: "ContestDomainOfInfluences");
    }
}
