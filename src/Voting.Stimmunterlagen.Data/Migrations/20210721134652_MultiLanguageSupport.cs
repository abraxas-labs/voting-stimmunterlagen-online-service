// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class MultiLanguageSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EVoting",
            table: "PoliticalBusinesses");

        migrationBuilder.DropColumn(
            name: "OfficialDescription",
            table: "PoliticalBusinesses");

        migrationBuilder.DropColumn(
            name: "ShortDescription",
            table: "PoliticalBusinesses");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Contests");

        migrationBuilder.CreateTable(
            name: "ContestTranslation",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Language = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestTranslation", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestTranslation_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PoliticalBusinessTranslation",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Language = table.Column<string>(type: "text", nullable: false),
                OfficialDescription = table.Column<string>(type: "text", nullable: false),
                ShortDescription = table.Column<string>(type: "text", nullable: false),
                PoliticalBusinessId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PoliticalBusinessTranslation", x => x.Id);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessTranslation_PoliticalBusinesses_PoliticalB~",
                    column: x => x.PoliticalBusinessId,
                    principalTable: "PoliticalBusinesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContestTranslation_ContestId_Language",
            table: "ContestTranslation",
            columns: new[] { "ContestId", "Language" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessTranslation_PoliticalBusinessId_Language",
            table: "PoliticalBusinessTranslation",
            columns: new[] { "PoliticalBusinessId", "Language" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ContestTranslation");

        migrationBuilder.DropTable(
            name: "PoliticalBusinessTranslation");

        migrationBuilder.AddColumn<bool>(
            name: "EVoting",
            table: "PoliticalBusinesses",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "OfficialDescription",
            table: "PoliticalBusinesses",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "ShortDescription",
            table: "PoliticalBusinesses",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Contests",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }
}
