// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddAttachmentCommentsAndStation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Station",
            table: "Attachments",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "AttachmentComment",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedBy_SecureConnectId = table.Column<string>(type: "text", nullable: false),
                CreatedBy_FirstName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_LastName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_UserName = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AttachmentComment", x => x.Id);
                table.ForeignKey(
                    name: "FK_AttachmentComment_Attachments_AttachmentId",
                    column: x => x.AttachmentId,
                    principalTable: "Attachments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AttachmentComment_AttachmentId",
            table: "AttachmentComment",
            column: "AttachmentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AttachmentComment");

        migrationBuilder.DropColumn(
            name: "Station",
            table: "Attachments");
    }
}
