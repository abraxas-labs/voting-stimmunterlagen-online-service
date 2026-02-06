// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddDomainOfInfluenceVoterDuplicates : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoterDuplicates");

        migrationBuilder.DropColumn(
            name: "HasVoterDuplicates",
            table: "VoterLists");

        migrationBuilder.AddColumn<Guid>(
            name: "VoterDuplicateId",
            table: "Voters",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "VotingCardPrintDisabled",
            table: "Voters",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "NumberOfVotingCards",
            table: "VoterLists",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql(@"UPDATE ""VoterLists"" SET ""NumberOfVotingCards"" = ""NumberOfVoters""");

        migrationBuilder.AddColumn<bool>(
            name: "ElectoralRegisterMultipleEnabled",
            table: "DomainOfInfluences",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "ElectoralRegisterMultipleEnabled",
            table: "ContestDomainOfInfluences",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceVoterDuplicates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                DateOfBirth = table.Column<string>(type: "text", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceVoterDuplicates", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceVoterDuplicates_ContestDomainOfInfluences_~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Voters_VoterDuplicateId",
            table: "Voters",
            column: "VoterDuplicateId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVoterDuplicates_DomainOfInfluenceId",
            table: "DomainOfInfluenceVoterDuplicates",
            column: "DomainOfInfluenceId");

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_DomainOfInfluenceVoterDuplicates_VoterDuplicateId",
            table: "Voters",
            column: "VoterDuplicateId",
            principalTable: "DomainOfInfluenceVoterDuplicates",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Voters_DomainOfInfluenceVoterDuplicates_VoterDuplicateId",
            table: "Voters");

        migrationBuilder.DropTable(
            name: "DomainOfInfluenceVoterDuplicates");

        migrationBuilder.DropIndex(
            name: "IX_Voters_VoterDuplicateId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "VoterDuplicateId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "VotingCardPrintDisabled",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "NumberOfVotingCards",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "ElectoralRegisterMultipleEnabled",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ElectoralRegisterMultipleEnabled",
            table: "ContestDomainOfInfluences");

        migrationBuilder.AddColumn<bool>(
            name: "HasVoterDuplicates",
            table: "VoterLists",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "VoterDuplicates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ListId = table.Column<Guid>(type: "uuid", nullable: false),
                DateOfBirth = table.Column<string>(type: "text", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                PersonId = table.Column<string>(type: "text", nullable: false),
                Sex = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterDuplicates", x => x.Id);
                table.ForeignKey(
                    name: "FK_VoterDuplicates_VoterLists_ListId",
                    column: x => x.ListId,
                    principalTable: "VoterLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VoterDuplicates_ListId",
            table: "VoterDuplicates",
            column: "ListId");
    }
}
