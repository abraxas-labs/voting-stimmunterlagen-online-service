// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingCardPrintFileExportJob : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_PostageCode",
            table: "Voters",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Town",
            table: "Voters",
            type: "character varying(40)",
            maxLength: 40,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Street",
            table: "Voters",
            type: "character varying(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_Name",
            table: "Voters",
            type: "character varying(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_AddOn1",
            table: "Voters",
            type: "character varying(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine2",
            table: "Voters",
            type: "character varying(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine1",
            table: "Voters",
            type: "character varying(60)",
            maxLength: 60,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PersonIdCategory",
            table: "Voters",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "PersonId",
            table: "Voters",
            type: "character varying(36)",
            maxLength: 36,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "MunicipalityName",
            table: "Voters",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "VoterDomainOfInfluence",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Id",
            table: "VoterDomainOfInfluence",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.CreateTable(
            name: "VotingCardPrintFileExportJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                State = table.Column<int>(type: "integer", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false),
                Started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Failed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Runner = table.Column<string>(type: "text", nullable: false),
                VotingCardGeneratorJobId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VotingCardPrintFileExportJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_VotingCardPrintFileExportJobs_VotingCardGeneratorJobs_Votin~",
                    column: x => x.VotingCardGeneratorJobId,
                    principalTable: "VotingCardGeneratorJobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VotingCardPrintFileExportJobs_VotingCardGeneratorJobId",
            table: "VotingCardPrintFileExportJobs",
            column: "VotingCardGeneratorJobId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VotingCardPrintFileExportJobs");

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_PostageCode",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Town",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(40)",
            oldMaxLength: 40,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Street",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(60)",
            oldMaxLength: 60,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_Name",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(60)",
            oldMaxLength: 60,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_Organisation_AddOn1",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(60)",
            oldMaxLength: 60,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine2",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(60)",
            oldMaxLength: 60,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "SwissAbroadPerson_Extension_Authority_AddressLine1",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(60)",
            oldMaxLength: 60,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "PersonIdCategory",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "PersonId",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(36)",
            oldMaxLength: 36);

        migrationBuilder.AlterColumn<string>(
            name: "MunicipalityName",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(40)",
            oldMaxLength: 40);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "VoterDomainOfInfluence",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "Id",
            table: "VoterDomainOfInfluence",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50);
    }
}
