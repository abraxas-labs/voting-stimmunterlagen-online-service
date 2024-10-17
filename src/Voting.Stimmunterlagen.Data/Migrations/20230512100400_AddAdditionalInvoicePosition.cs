// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddAdditionalInvoicePosition : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SapCustomerOrderNumber",
            table: "DomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Runner",
            table: "ContestEVotingExportJobs",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "SapCustomerOrderNumber",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateTable(
            name: "AdditionalInvoicePositions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                AmountCentime = table.Column<int>(type: "integer", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy_SecureConnectId = table.Column<string>(type: "text", nullable: false),
                CreatedBy_FirstName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_LastName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_UserName = table.Column<string>(type: "text", nullable: false),
                Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ModifiedBy_SecureConnectId = table.Column<string>(type: "text", nullable: false),
                ModifiedBy_FirstName = table.Column<string>(type: "text", nullable: false),
                ModifiedBy_LastName = table.Column<string>(type: "text", nullable: false),
                ModifiedBy_UserName = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdditionalInvoicePositions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AdditionalInvoicePositions_ContestDomainOfInfluences_Domain~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AdditionalInvoicePositions_DomainOfInfluenceId",
            table: "AdditionalInvoicePositions",
            column: "DomainOfInfluenceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AdditionalInvoicePositions");

        migrationBuilder.DropColumn(
            name: "SapCustomerOrderNumber",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "Runner",
            table: "ContestEVotingExportJobs");

        migrationBuilder.DropColumn(
            name: "SapCustomerOrderNumber",
            table: "ContestDomainOfInfluences");
    }
}
