// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RenameAttachmentRequiredToRequiredForVoterListsCount : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "RequiredCount",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "RequiredForVoterListsCount");

        migrationBuilder.RenameColumn(
            name: "TotalRequiredCount",
            table: "Attachments",
            newName: "TotalRequiredForVoterListsCount");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "RequiredForVoterListsCount",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "RequiredCount");

        migrationBuilder.RenameColumn(
            name: "TotalRequiredForVoterListsCount",
            table: "Attachments",
            newName: "TotalRequiredCount");
    }
}
