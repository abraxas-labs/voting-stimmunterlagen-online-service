// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVoterDomainOfInfluence : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDomainOfInfluenceChurch",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDomainOfInfluenceSchool",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDomainOfInfluenceChurch",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "DataConfiguration_IncludeDomainOfInfluenceSchool",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "VoterDomainOfInfluence",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceType = table.Column<int>(type: "integer", nullable: false),
                DomainOfInfluenceIdentification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                DomainOfInfluenceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VoterId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterDomainOfInfluence", x => x.Id);
                table.ForeignKey(
                    name: "FK_VoterDomainOfInfluence_Voters_VoterId",
                    column: x => x.VoterId,
                    principalTable: "Voters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VoterDomainOfInfluence_VoterId",
            table: "VoterDomainOfInfluence",
            column: "VoterId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoterDomainOfInfluence");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDomainOfInfluenceChurch",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDomainOfInfluenceSchool",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDomainOfInfluenceChurch",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DataConfiguration_IncludeDomainOfInfluenceSchool",
            table: "ContestVotingCardLayouts");
    }
}
