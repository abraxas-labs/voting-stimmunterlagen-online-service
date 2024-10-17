// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddDomainOfInfluenceDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "LogoRef",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingAway",
            table: "DomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingMethod",
            table: "DomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingReturn",
            table: "DomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressAddition",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressLine1",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressLine2",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_City",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_Country",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_Street",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_ZipCode",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LogoRef",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingAway",
            table: "ContestDomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingMethod",
            table: "ContestDomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingReturn",
            table: "ContestDomainOfInfluences",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressAddition",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressLine1",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_AddressLine2",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_City",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_Country",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_Street",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ReturnAddress_ZipCode",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LogoRef",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingAway",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingMethod",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingReturn",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressAddition",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressLine1",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressLine2",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_City",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_Country",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_Street",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_ZipCode",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "LogoRef",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingAway",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingMethod",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingReturn",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressAddition",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressLine1",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_AddressLine2",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_City",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_Country",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_Street",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "ReturnAddress_ZipCode",
            table: "ContestDomainOfInfluences");
    }
}
