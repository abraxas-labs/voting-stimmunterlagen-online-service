// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DomainOfInfluences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ShortName = table.Column<string>(type: "text", nullable: false),
                AuthorityName = table.Column<string>(type: "text", nullable: false),
                SecureConnectId = table.Column<string>(type: "text", nullable: false),
                ResponsibleForVotingCards = table.Column<bool>(type: "boolean", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Canton = table.Column<int>(type: "integer", nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluences", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluences_DomainOfInfluences_ParentId",
                    column: x => x.ParentId,
                    principalTable: "DomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EventProcessingStates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LastProcessedEventNumber = table.Column<long>(type: "bigint", nullable: false),
                LatestEverProcessedEventNumber = table.Column<long>(type: "bigint", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventProcessingStates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceHierarchyEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                ParentDomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceHierarchyEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceHierarchyEntries_DomainOfInfluences_Domain~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "DomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceHierarchyEntries_DomainOfInfluences_Parent~",
                    column: x => x.ParentDomainOfInfluenceId,
                    principalTable: "DomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ContestDomainOfInfluenceHierarchyEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                ParentDomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestDomainOfInfluenceHierarchyEntries", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Contests",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateTime>(type: "date", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: true),
                TestingPhaseEnded = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Contests", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContestDomainOfInfluences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                BasisDomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ShortName = table.Column<string>(type: "text", nullable: false),
                AuthorityName = table.Column<string>(type: "text", nullable: false),
                SecureConnectId = table.Column<string>(type: "text", nullable: false),
                ResponsibleForVotingCards = table.Column<bool>(type: "boolean", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                Canton = table.Column<int>(type: "integer", nullable: false),
                ParentId = table.Column<Guid>(type: "uuid", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestDomainOfInfluences", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestDomainOfInfluences_ContestDomainOfInfluences_ParentId",
                    column: x => x.ParentId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ContestDomainOfInfluences_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PoliticalBusinesses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PoliticalBusinessNumber = table.Column<string>(type: "text", nullable: false),
                OfficialDescription = table.Column<string>(type: "text", nullable: false),
                ShortDescription = table.Column<string>(type: "text", nullable: false),
                InternalDescription = table.Column<string>(type: "text", nullable: false),
                EVoting = table.Column<bool>(type: "boolean", nullable: false),
                Active = table.Column<bool>(type: "boolean", nullable: false),
                SwissAbroadVotingRight = table.Column<int>(type: "integer", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                PoliticalBusinessType = table.Column<int>(type: "integer", nullable: false),
                Approved = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PoliticalBusinesses", x => x.Id);
                table.ForeignKey(
                    name: "FK_PoliticalBusinesses_ContestDomainOfInfluences_DomainOfInflu~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PoliticalBusinesses_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "StepStates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                Step = table.Column<int>(type: "integer", nullable: false),
                Approved = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StepStates", x => x.Id);
                table.ForeignKey(
                    name: "FK_StepStates_ContestDomainOfInfluences_DomainOfInfluenceId",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PoliticalBusinessPermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                SecureConnectId = table.Column<string>(type: "text", nullable: false),
                PoliticalBusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PoliticalBusinessPermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessPermissions_ContestDomainOfInfluences_Doma~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessPermissions_PoliticalBusinesses_PoliticalB~",
                    column: x => x.PoliticalBusinessId,
                    principalTable: "PoliticalBusinesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceHierarchyEntries_DomainOfInfluenceI~",
            table: "ContestDomainOfInfluenceHierarchyEntries",
            columns: new[] { "DomainOfInfluenceId", "ParentDomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceHierarchyEntries_ParentDomainOfInfl~",
            table: "ContestDomainOfInfluenceHierarchyEntries",
            column: "ParentDomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluences_ContestId",
            table: "ContestDomainOfInfluences",
            column: "ContestId");

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluences_ParentId",
            table: "ContestDomainOfInfluences",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Contests_DomainOfInfluenceId",
            table: "Contests",
            column: "DomainOfInfluenceId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Contests_TestingPhaseEnded",
            table: "Contests",
            column: "TestingPhaseEnded");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceHierarchyEntries_DomainOfInfluenceId_Paren~",
            table: "DomainOfInfluenceHierarchyEntries",
            columns: new[] { "DomainOfInfluenceId", "ParentDomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceHierarchyEntries_ParentDomainOfInfluenceId",
            table: "DomainOfInfluenceHierarchyEntries",
            column: "ParentDomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluences_ParentId",
            table: "DomainOfInfluences",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinesses_ContestId",
            table: "PoliticalBusinesses",
            column: "ContestId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinesses_DomainOfInfluenceId",
            table: "PoliticalBusinesses",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessPermissions_DomainOfInfluenceId",
            table: "PoliticalBusinessPermissions",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessPermissions_PoliticalBusinessId_DomainOfIn~",
            table: "PoliticalBusinessPermissions",
            columns: new[] { "PoliticalBusinessId", "DomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_StepStates_DomainOfInfluenceId_Step",
            table: "StepStates",
            columns: new[] { "DomainOfInfluenceId", "Step" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_ContestDomainOfInfluenceHierarchyEntries_ContestDomainOfIn~1",
            table: "ContestDomainOfInfluenceHierarchyEntries",
            column: "ParentDomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ContestDomainOfInfluenceHierarchyEntries_ContestDomainOfInf~",
            table: "ContestDomainOfInfluenceHierarchyEntries",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Contests_ContestDomainOfInfluences_DomainOfInfluenceId",
            table: "Contests");

        migrationBuilder.DropTable(
            name: "ContestDomainOfInfluenceHierarchyEntries");

        migrationBuilder.DropTable(
            name: "DomainOfInfluenceHierarchyEntries");

        migrationBuilder.DropTable(
            name: "EventProcessingStates");

        migrationBuilder.DropTable(
            name: "PoliticalBusinessPermissions");

        migrationBuilder.DropTable(
            name: "StepStates");

        migrationBuilder.DropTable(
            name: "DomainOfInfluences");

        migrationBuilder.DropTable(
            name: "PoliticalBusinesses");

        migrationBuilder.DropTable(
            name: "ContestDomainOfInfluences");

        migrationBuilder.DropTable(
            name: "Contests");
    }
}
