// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddDomainOfInfluenceAttachmentCount : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Count",
            table: "Attachments");

        migrationBuilder.AlterColumn<DateTime>(
            name: "DeliveryPlannedOn",
            table: "Attachments",
            type: "date",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "date",
            oldNullable: true);

        migrationBuilder.Sql("UPDATE \"Attachments\" SET \"Format\" = 2 WHERE \"Format\" = 3");

        migrationBuilder.CreateTable(
            name: "DomainOfInfluenceAttachmentCounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                Count = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DomainOfInfluenceAttachmentCounts", x => x.Id);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceAttachmentCounts_Attachments_AttachmentId",
                    column: x => x.AttachmentId,
                    principalTable: "Attachments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_DomainOfInfluenceAttachmentCounts_ContestDomainOfInfluences~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceAttachmentCounts_AttachmentId",
            table: "DomainOfInfluenceAttachmentCounts",
            column: "AttachmentId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceAttachmentCounts_DomainOfInfluenceId_Attac~",
            table: "DomainOfInfluenceAttachmentCounts",
            columns: new[] { "DomainOfInfluenceId", "AttachmentId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DomainOfInfluenceAttachmentCounts");

        migrationBuilder.AlterColumn<DateTime>(
            name: "DeliveryPlannedOn",
            table: "Attachments",
            type: "date",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "date");

        migrationBuilder.AddColumn<int>(
            name: "Count",
            table: "Attachments",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
