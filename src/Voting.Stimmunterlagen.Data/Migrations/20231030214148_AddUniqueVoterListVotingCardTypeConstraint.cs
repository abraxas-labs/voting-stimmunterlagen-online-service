// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddUniqueVoterListVotingCardTypeConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists");

        migrationBuilder.CreateIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists",
            columns: new[] { "ImportId", "VotingCardType" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists");

        migrationBuilder.CreateIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists",
            columns: new[] { "ImportId", "VotingCardType" });
    }
}
