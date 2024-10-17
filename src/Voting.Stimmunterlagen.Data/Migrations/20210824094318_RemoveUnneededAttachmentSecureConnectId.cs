// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RemoveUnneededAttachmentSecureConnectId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SecureConnectId",
            table: "Attachments");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SecureConnectId",
            table: "Attachments",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }
}
