// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class MoveTemplateDataFieldValueFieldTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TemplateDataFieldValues_ContestDomainOfInfluences_DomainOfI~",
            table: "TemplateDataFieldValues");

        migrationBuilder.RenameColumn(
            name: "DomainOfInfluenceId",
            table: "TemplateDataFieldValues",
            newName: "LayoutId");

        migrationBuilder.RenameIndex(
            name: "IX_TemplateDataFieldValues_FieldId_DomainOfInfluenceId",
            table: "TemplateDataFieldValues",
            newName: "IX_TemplateDataFieldValues_FieldId_LayoutId");

        migrationBuilder.RenameIndex(
            name: "IX_TemplateDataFieldValues_DomainOfInfluenceId",
            table: "TemplateDataFieldValues",
            newName: "IX_TemplateDataFieldValues_LayoutId");

        migrationBuilder.AddForeignKey(
            name: "FK_TemplateDataFieldValues_DomainOfInfluenceVotingCardLayouts_~",
            table: "TemplateDataFieldValues",
            column: "LayoutId",
            principalTable: "DomainOfInfluenceVotingCardLayouts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TemplateDataFieldValues_DomainOfInfluenceVotingCardLayouts_~",
            table: "TemplateDataFieldValues");

        migrationBuilder.RenameColumn(
            name: "LayoutId",
            table: "TemplateDataFieldValues",
            newName: "DomainOfInfluenceId");

        migrationBuilder.RenameIndex(
            name: "IX_TemplateDataFieldValues_LayoutId",
            table: "TemplateDataFieldValues",
            newName: "IX_TemplateDataFieldValues_DomainOfInfluenceId");

        migrationBuilder.RenameIndex(
            name: "IX_TemplateDataFieldValues_FieldId_LayoutId",
            table: "TemplateDataFieldValues",
            newName: "IX_TemplateDataFieldValues_FieldId_DomainOfInfluenceId");

        migrationBuilder.AddForeignKey(
            name: "FK_TemplateDataFieldValues_ContestDomainOfInfluences_DomainOfI~",
            table: "TemplateDataFieldValues",
            column: "DomainOfInfluenceId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
