// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddTemplateData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");
        migrationBuilder.AddColumn<int>(
            name: "TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.DropColumn(
            name: "OverriddenTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");
        migrationBuilder.AddColumn<int>(
            name: "OverriddenTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "DomainOfInfluenceTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.DropColumn(
            name: "TemplateId",
            table: "ContestVotingCardLayouts");
        migrationBuilder.AddColumn<int>(
            name: "TemplateId",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "TemplateDataContainers",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Key = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TemplateDataContainers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Templates",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                Categories = table.Column<List<string>>(type: "text[]", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Templates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TemplateDataFields",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Key = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ContainerId = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TemplateDataFields", x => x.Id);
                table.ForeignKey(
                    name: "FK_TemplateDataFields_TemplateDataContainers_ContainerId",
                    column: x => x.ContainerId,
                    principalTable: "TemplateDataContainers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TemplateTemplateDataContainer",
            columns: table => new
            {
                DataContainersId = table.Column<int>(type: "integer", nullable: false),
                TemplatesId = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TemplateTemplateDataContainer", x => new { x.DataContainersId, x.TemplatesId });
                table.ForeignKey(
                    name: "FK_TemplateTemplateDataContainer_TemplateDataContainers_DataCo~",
                    column: x => x.DataContainersId,
                    principalTable: "TemplateDataContainers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TemplateTemplateDataContainer_Templates_TemplatesId",
                    column: x => x.TemplatesId,
                    principalTable: "Templates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TemplateDataFieldValues",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Value = table.Column<string>(type: "text", nullable: false),
                FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TemplateDataFieldValues", x => x.Id);
                table.ForeignKey(
                    name: "FK_TemplateDataFieldValues_ContestDomainOfInfluences_DomainOfI~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_TemplateDataFieldValues_TemplateDataFields_FieldId",
                    column: x => x.FieldId,
                    principalTable: "TemplateDataFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_DomainOfInfluenceTemplat~",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "DomainOfInfluenceTemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_OverriddenTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "OverriddenTemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "TemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_ContestVotingCardLayouts_TemplateId",
            table: "ContestVotingCardLayouts",
            column: "TemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_TemplateDataFields_ContainerId_Key",
            table: "TemplateDataFields",
            columns: new[] { "ContainerId", "Key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TemplateDataFieldValues_DomainOfInfluenceId",
            table: "TemplateDataFieldValues",
            column: "DomainOfInfluenceId");

        migrationBuilder.CreateIndex(
            name: "IX_TemplateDataFieldValues_FieldId_DomainOfInfluenceId",
            table: "TemplateDataFieldValues",
            columns: new[] { "FieldId", "DomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TemplateTemplateDataContainer_TemplatesId",
            table: "TemplateTemplateDataContainer",
            column: "TemplatesId");

        migrationBuilder.AddForeignKey(
            name: "FK_ContestVotingCardLayouts_Templates_TemplateId",
            table: "ContestVotingCardLayouts",
            column: "TemplateId",
            principalTable: "Templates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_DomainOfInflue~",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "DomainOfInfluenceTemplateId",
            principalTable: "Templates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_OverriddenTemp~",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "OverriddenTemplateId",
            principalTable: "Templates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            column: "TemplateId",
            principalTable: "Templates",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ContestVotingCardLayouts_Templates_TemplateId",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_DomainOfInflue~",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_OverriddenTemp~",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropForeignKey(
            name: "FK_DomainOfInfluenceVotingCardLayouts_Templates_TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropTable(
            name: "TemplateDataFieldValues");

        migrationBuilder.DropTable(
            name: "TemplateTemplateDataContainer");

        migrationBuilder.DropTable(
            name: "TemplateDataFields");

        migrationBuilder.DropTable(
            name: "Templates");

        migrationBuilder.DropTable(
            name: "TemplateDataContainers");

        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_DomainOfInfluenceTemplat~",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_OverriddenTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluenceVotingCardLayouts_TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropIndex(
            name: "IX_ContestVotingCardLayouts_TemplateId",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "DomainOfInfluenceTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.AlterColumn<string>(
            name: "TemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "text",
            nullable: false,
            defaultValue: string.Empty,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "OverriddenTemplateId",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "text",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "TemplateId",
            table: "ContestVotingCardLayouts",
            type: "text",
            nullable: false,
            defaultValue: string.Empty,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);
    }
}
