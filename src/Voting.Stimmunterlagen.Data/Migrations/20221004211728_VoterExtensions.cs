// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class VoterExtensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "DateOfBirth",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "MunicipalityName",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "PersonId",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "PersonIdCategory",
            table: "Voters",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<int>(
            name: "Sex",
            table: "Voters",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "SwissAbroadPerson_DateOfRegistration",
            table: "Voters",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line1",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line2",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line3",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line4",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line5",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line6",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Address_Line7",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine1",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine2",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Country_Iso2",
            table: "Voters",
            type: "character varying(2)",
            maxLength: 2,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Country_Name",
            table: "Voters",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_AddOn1",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_Name",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Street",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SwissAbroadPerson_Extension_Authority_SwissZipCode",
            table: "Voters",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Town",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_Extension_PostageCode",
            table: "Voters",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_ResidenceCountry_Iso2",
            table: "Voters",
            type: "character varying(2)",
            maxLength: 2,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissAbroadPerson_ResidenceCountry_Name",
            table: "Voters",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "VoterType",
            table: "Voters",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "VoterDomainOfInfluence",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterDomainOfInfluence", x => new { x.VoterId, x.Id });
                table.ForeignKey(
                    name: "FK_VoterDomainOfInfluence_Voters_VoterId",
                    column: x => x.VoterId,
                    principalTable: "Voters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoterDomainOfInfluence");

        migrationBuilder.DropColumn(
            name: "DateOfBirth",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "MunicipalityName",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "PersonId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "PersonIdCategory",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "Sex",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_DateOfRegistration",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line1",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line2",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line3",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line4",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line5",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line6",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Address_Line7",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine1",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine2",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Country_Iso2",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Country_Name",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_AddOn1",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_Name",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Street",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_SwissZipCode",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_Authority_Town",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_Extension_PostageCode",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_ResidenceCountry_Iso2",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "SwissAbroadPerson_ResidenceCountry_Name",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "VoterType",
            table: "Voters");
    }
}
