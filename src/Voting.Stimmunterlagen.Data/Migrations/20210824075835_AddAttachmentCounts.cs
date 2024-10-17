// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddAttachmentCounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Count",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "RequiredCount");

        migrationBuilder.AddColumn<int>(
            name: "OrderedCount",
            table: "DomainOfInfluenceAttachmentCounts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "TotalOrderedCount",
            table: "Attachments",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "TotalRequiredCount",
            table: "Attachments",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OrderedCount",
            table: "DomainOfInfluenceAttachmentCounts");

        migrationBuilder.DropColumn(
            name: "TotalOrderedCount",
            table: "Attachments");

        migrationBuilder.DropColumn(
            name: "TotalRequiredCount",
            table: "Attachments");

        migrationBuilder.RenameColumn(
            name: "RequiredCount",
            table: "DomainOfInfluenceAttachmentCounts",
            newName: "Count");
    }
}
