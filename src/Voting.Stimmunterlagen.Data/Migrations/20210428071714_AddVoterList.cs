// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVoterList : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VoterLists",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                VotingCardType = table.Column<int>(type: "integer", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false),
                NumberOfVoters = table.Column<int>(type: "integer", nullable: false),
                LastUpdate = table.Column<DateTime>(type: "date", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Index = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterLists", x => x.Id);
                table.ForeignKey(
                    name: "FK_VoterLists_ContestDomainOfInfluences_DomainOfInfluenceId",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PoliticalBusinessVoterListEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PoliticalBusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                VoterListId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PoliticalBusinessVoterListEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessVoterListEntries_PoliticalBusinesses_Polit~",
                    column: x => x.PoliticalBusinessId,
                    principalTable: "PoliticalBusinesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessVoterListEntries_VoterLists_VoterListId",
                    column: x => x.VoterListId,
                    principalTable: "VoterLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Voters",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "text", nullable: true),
                FirstName = table.Column<string>(type: "text", nullable: true),
                LastName = table.Column<string>(type: "text", nullable: false),
                AddressLine1 = table.Column<string>(type: "text", nullable: true),
                AddressLine2 = table.Column<string>(type: "text", nullable: true),
                Street = table.Column<string>(type: "text", nullable: false),
                HouseNumber = table.Column<string>(type: "text", nullable: true),
                DwellingNumber = table.Column<string>(type: "text", nullable: true),
                Locality = table.Column<string>(type: "text", nullable: true),
                Town = table.Column<string>(type: "text", nullable: false),
                SwissZipCode = table.Column<int>(type: "integer", nullable: true),
                ForeignZipCode = table.Column<string>(type: "text", nullable: true),
                Country_Iso2 = table.Column<string>(type: "text", nullable: true),
                Country_Name = table.Column<string>(type: "text", nullable: false),
                ListId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Voters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Voters_VoterLists_ListId",
                    column: x => x.ListId,
                    principalTable: "VoterLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessVoterListEntries_PoliticalBusinessId_Voter~",
            table: "PoliticalBusinessVoterListEntries",
            columns: new[] { "PoliticalBusinessId", "VoterListId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessVoterListEntries_VoterListId",
            table: "PoliticalBusinessVoterListEntries",
            column: "VoterListId");

        migrationBuilder.CreateIndex(
            name: "IX_VoterLists_DomainOfInfluenceId",
            table: "VoterLists",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_Voters_ListId",
            table: "Voters",
            column: "ListId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PoliticalBusinessVoterListEntries");

        migrationBuilder.DropTable(
            name: "Voters");

        migrationBuilder.DropTable(
            name: "VoterLists");
    }
}
