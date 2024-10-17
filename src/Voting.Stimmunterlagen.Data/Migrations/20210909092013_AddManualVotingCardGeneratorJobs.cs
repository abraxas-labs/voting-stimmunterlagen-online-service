// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddManualVotingCardGeneratorJobs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Voters_VoterLists_ListId",
            table: "Voters");

        migrationBuilder.AlterColumn<string>(
            name: "Town",
            table: "Voters",
            type: "character varying(40)",
            maxLength: 40,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Voters",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Street",
            table: "Voters",
            type: "character varying(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Locality",
            table: "Voters",
            type: "character varying(40)",
            maxLength: 40,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "ListId",
            table: "Voters",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AlterColumn<string>(
            name: "LastName",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "HouseNumber",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ForeignZipCode",
            table: "Voters",
            type: "character varying(40)",
            maxLength: 40,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DwellingNumber",
            table: "Voters",
            type: "character varying(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Country_Name",
            table: "Voters",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Country_Iso2",
            table: "Voters",
            type: "character varying(2)",
            maxLength: 2,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "AddressLine2",
            table: "Voters",
            type: "character varying(150)",
            maxLength: 150,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "AddressLine1",
            table: "Voters",
            type: "character varying(150)",
            maxLength: 150,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ManualJobId",
            table: "Voters",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardType",
            table: "Voters",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Bfs",
            table: "DomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Bfs",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateTable(
            name: "ManualVotingCardGeneratorJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedBy_SecureConnectId = table.Column<string>(type: "text", nullable: false),
                CreatedBy_FirstName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_LastName = table.Column<string>(type: "text", nullable: false),
                CreatedBy_UserName = table.Column<string>(type: "text", nullable: false),
                LayoutId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ManualVotingCardGeneratorJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_ManualVotingCardGeneratorJobs_DomainOfInfluenceVotingCardLa~",
                    column: x => x.LayoutId,
                    principalTable: "DomainOfInfluenceVotingCardLayouts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Voters_ManualJobId",
            table: "Voters",
            column: "ManualJobId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ManualVotingCardGeneratorJobs_LayoutId",
            table: "ManualVotingCardGeneratorJobs",
            column: "LayoutId");

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_ManualVotingCardGeneratorJobs_ManualJobId",
            table: "Voters",
            column: "ManualJobId",
            principalTable: "ManualVotingCardGeneratorJobs",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_VoterLists_ListId",
            table: "Voters",
            column: "ListId",
            principalTable: "VoterLists",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Voters_ManualVotingCardGeneratorJobs_ManualJobId",
            table: "Voters");

        migrationBuilder.DropForeignKey(
            name: "FK_Voters_VoterLists_ListId",
            table: "Voters");

        migrationBuilder.DropTable(
            name: "ManualVotingCardGeneratorJobs");

        migrationBuilder.DropIndex(
            name: "IX_Voters_ManualJobId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "ManualJobId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "VotingCardType",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "Bfs",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "Bfs",
            table: "ContestDomainOfInfluences");

        migrationBuilder.AlterColumn<string>(
            name: "Town",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(40)",
            oldMaxLength: 40);

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Street",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(150)",
            oldMaxLength: 150);

        migrationBuilder.AlterColumn<string>(
            name: "Locality",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(40)",
            oldMaxLength: 40,
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "ListId",
            table: "Voters",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "LastName",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(30)",
            oldMaxLength: 30);

        migrationBuilder.AlterColumn<string>(
            name: "HouseNumber",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ForeignZipCode",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(40)",
            oldMaxLength: 40,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "FirstName",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DwellingNumber",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Country_Name",
            table: "Voters",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "Country_Iso2",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(2)",
            oldMaxLength: 2,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "AddressLine2",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(150)",
            oldMaxLength: 150,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "AddressLine1",
            table: "Voters",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(150)",
            oldMaxLength: 150,
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_VoterLists_ListId",
            table: "Voters",
            column: "ListId",
            principalTable: "VoterLists",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
