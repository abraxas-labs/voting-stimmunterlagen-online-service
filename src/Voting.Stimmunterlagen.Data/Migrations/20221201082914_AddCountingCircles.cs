// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddCountingCircles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoterDomainOfInfluence");

        migrationBuilder.CreateTable(
            name: "ContestCountingCircles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                BasisCountingCircleId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Bfs = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestCountingCircles", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestCountingCircles_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CountingCircles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Bfs = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CountingCircles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContestDomainOfInfluenceCountingCircles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                CountingCircleId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestDomainOfInfluenceCountingCircles", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestDomainOfInfluenceCountingCircles_ContestCountingCirc~",
                    column: x => x.CountingCircleId,
                    principalTable: "ContestCountingCircles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ContestDomainOfInfluenceCountingCircles_ContestDomainOfInfl~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceCountingCircles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                CountingCircleId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceCountingCircles", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceCountingCircles_CountingCircles_CountingCi~",
                    column: x => x.CountingCircleId,
                    principalTable: "CountingCircles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceCountingCircles_DomainOfInfluences_DomainO~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "DomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContestCountingCircles_ContestId",
            table: "ContestCountingCircles",
            column: "ContestId");

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_CountingCircleId_Do~",
            table: "ContestDomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_DomainOfInfluenceId",
            table: "ContestDomainOfInfluenceCountingCircles",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceCountingCircles_CountingCircleId_DomainOfI~",
            table: "DomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceCountingCircles_DomainOfInfluenceId",
            table: "DomainOfInfluenceCountingCircles",
            column: "DomainOfInfluenceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ContestDomainOfInfluenceCountingCircles");

        migrationBuilder.DropTable(
            name: "DomainOfInfluenceCountingCircles");

        migrationBuilder.DropTable(
            name: "ContestCountingCircles");

        migrationBuilder.DropTable(
            name: "CountingCircles");

        migrationBuilder.CreateTable(
            name: "VoterDomainOfInfluence",
            columns: table => new
            {
                VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterDomainOfInfluence", x => new { x.VoterId, x.Id });
                table.ForeignKey(
                    name: "FK_VoterDomainOfInfluence_Voters_VoterId",
                    column: x => x.VoterId,
                    principalTable: "Voters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }
}
