// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddAttachment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Count = table.Column<int>(type: "integer", nullable: false),
                Format = table.Column<int>(type: "integer", nullable: false),
                Color = table.Column<string>(type: "text", nullable: false),
                Supplier = table.Column<string>(type: "text", nullable: false),
                SecureConnectId = table.Column<string>(type: "text", nullable: false),
                DeliveryPlannedOn = table.Column<DateTime>(type: "date", nullable: true),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Attachments_ContestDomainOfInfluences_DomainOfInfluenceId",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PoliticalBusinessAttachmentEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PoliticalBusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PoliticalBusinessAttachmentEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessAttachmentEntries_Attachments_AttachmentId",
                    column: x => x.AttachmentId,
                    principalTable: "Attachments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PoliticalBusinessAttachmentEntries_PoliticalBusinesses_Poli~",
                    column: x => x.PoliticalBusinessId,
                    principalTable: "PoliticalBusinesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_DomainOfInfluenceId",
            table: "Attachments",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessAttachmentEntries_AttachmentId",
            table: "PoliticalBusinessAttachmentEntries",
            column: "AttachmentId");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessAttachmentEntries_PoliticalBusinessId_Atta~",
            table: "PoliticalBusinessAttachmentEntries",
            columns: new[] { "PoliticalBusinessId", "AttachmentId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PoliticalBusinessAttachmentEntries");

        migrationBuilder.DropTable(
            name: "Attachments");
    }
}
