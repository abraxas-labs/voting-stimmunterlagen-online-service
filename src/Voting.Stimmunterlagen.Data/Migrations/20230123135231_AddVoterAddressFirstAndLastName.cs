// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVoterAddressFirstAndLastName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Voters",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: string.Empty,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AddressFirstName",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AddressLastName",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: string.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AddressFirstName",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "AddressLastName",
            table: "Voters");

        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Voters",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);
    }
}
