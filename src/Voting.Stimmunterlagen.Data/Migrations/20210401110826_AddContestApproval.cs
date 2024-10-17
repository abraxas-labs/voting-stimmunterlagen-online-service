// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddContestApproval : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PoliticalBusinessPermissions_PoliticalBusinessId_DomainOfIn~",
            table: "PoliticalBusinessPermissions");

        migrationBuilder.AddColumn<DateTime>(
            name: "Approved",
            table: "Contests",
            type: "timestamp without time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "AttachmentDeliveryDeadline",
            table: "Contests",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsSingleAttendeeContest",
            table: "Contests",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "PrintingCenterSignUpDeadline",
            table: "Contests",
            type: "date",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessPermissions_PoliticalBusinessId_DomainOfIn~",
            table: "PoliticalBusinessPermissions",
            columns: new[] { "PoliticalBusinessId", "DomainOfInfluenceId", "Role" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_PoliticalBusinessPermissions_PoliticalBusinessId_DomainOfIn~",
            table: "PoliticalBusinessPermissions");

        migrationBuilder.DropColumn(
            name: "Approved",
            table: "Contests");

        migrationBuilder.DropColumn(
            name: "AttachmentDeliveryDeadline",
            table: "Contests");

        migrationBuilder.DropColumn(
            name: "IsSingleAttendeeContest",
            table: "Contests");

        migrationBuilder.DropColumn(
            name: "PrintingCenterSignUpDeadline",
            table: "Contests");

        migrationBuilder.CreateIndex(
            name: "IX_PoliticalBusinessPermissions_PoliticalBusinessId_DomainOfIn~",
            table: "PoliticalBusinessPermissions",
            columns: new[] { "PoliticalBusinessId", "DomainOfInfluenceId" },
            unique: true);
    }
}
