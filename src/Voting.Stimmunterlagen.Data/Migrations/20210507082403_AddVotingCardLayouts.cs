// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingCardLayouts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "EVoting",
            table: "Contests",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "ContestVotingCardLayouts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContestId = table.Column<Guid>(type: "uuid", nullable: false),
                AllowCustom = table.Column<bool>(type: "boolean", nullable: false),
                TemplateId = table.Column<string>(type: "text", nullable: false),
                VotingCardType = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestVotingCardLayouts", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContestVotingCardLayouts_Contests_ContestId",
                    column: x => x.ContestId,
                    principalTable: "Contests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceVotingCardLayouts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                OverriddenTemplateId = table.Column<string>(type: "text", nullable: true),
                AllowCustom = table.Column<bool>(type: "boolean", nullable: false),
                TemplateId = table.Column<string>(type: "text", nullable: false),
                VotingCardType = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceVotingCardLayouts", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceVotingCardLayouts_ContestDomainOfInfluence~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ContestVotingCardLayouts_ContestId_VotingCardType",
            table: "ContestVotingCardLayouts",
            columns: new[] { "ContestId", "VotingCardType" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_DomainOfInfluenceId_Voti~",
            table: "DomainOfInfluenceVotingCardLayouts",
            columns: new[] { "DomainOfInfluenceId", "VotingCardType" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ContestVotingCardLayouts");

        migrationBuilder.DropTable(
            name: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "EVoting",
            table: "Contests");
    }
}
