// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RenameAttachmentOrderedToRequiredCount : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "OrderedCount",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "RequiredCount");

        migrationBuilder.RenameColumn(
            name: "TotalOrderedCount",
            table: "Attachments",
            newName: "TotalRequiredCount");

        migrationBuilder.AddColumn<int>(
            name: "OrderedCount",
            table: "Attachments",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OrderedCount",
            table: "Attachments");

        migrationBuilder.RenameColumn(
            name: "RequiredCount",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "OrderedCount");

        migrationBuilder.RenameColumn(
            name: "TotalRequiredCount",
            table: "Attachments",
            newName: "TotalOrderedCount");
    }
}
