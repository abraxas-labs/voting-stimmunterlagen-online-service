// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddDoiCcSourceId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluenceCountingCircles_CountingCircleId_DomainOfI~",
            table: "DomainOfInfluenceCountingCircles");

        migrationBuilder.DropIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_CountingCircleId_Do~",
            table: "ContestDomainOfInfluenceCountingCircles");

        migrationBuilder.AddColumn<Guid>(
            name: "SourceDomainOfInfluenceId",
            table: "DomainOfInfluenceCountingCircles",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "SourceDomainOfInfluenceId",
            table: "ContestDomainOfInfluenceCountingCircles",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceCountingCircles_CountingCircleId_DomainOfI~",
            table: "DomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId", "SourceDomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_CountingCircleId_Do~",
            table: "ContestDomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId", "SourceDomainOfInfluenceId" },
            unique: true);

        migrationBuilder.Sql(@"UPDATE ""DomainOfInfluenceCountingCircles"" SET ""SourceDomainOfInfluenceId"" = ""DomainOfInfluenceId""");
        migrationBuilder.Sql(@"UPDATE ""ContestDomainOfInfluenceCountingCircles"" SET ""SourceDomainOfInfluenceId"" = ""DomainOfInfluenceId""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluenceCountingCircles_CountingCircleId_DomainOfI~",
            table: "DomainOfInfluenceCountingCircles");

        migrationBuilder.DropIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_CountingCircleId_Do~",
            table: "ContestDomainOfInfluenceCountingCircles");

        migrationBuilder.DropColumn(
            name: "SourceDomainOfInfluenceId",
            table: "DomainOfInfluenceCountingCircles");

        migrationBuilder.DropColumn(
            name: "SourceDomainOfInfluenceId",
            table: "ContestDomainOfInfluenceCountingCircles");

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluenceCountingCircles_CountingCircleId_DomainOfI~",
            table: "DomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluenceCountingCircles_CountingCircleId_Do~",
            table: "ContestDomainOfInfluenceCountingCircles",
            columns: new[] { "CountingCircleId", "DomainOfInfluenceId" },
            unique: true);
    }
}
