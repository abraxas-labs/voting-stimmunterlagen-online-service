// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddDoiSwissPostData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_InvoiceReferenceNumber",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_OrderNumber",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_InvoiceReferenceNumber",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_OrderNumber",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_InvoiceReferenceNumber",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_OrderNumber",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_InvoiceReferenceNumber",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_OrderNumber",
            table: "ContestDomainOfInfluences");
    }
}
