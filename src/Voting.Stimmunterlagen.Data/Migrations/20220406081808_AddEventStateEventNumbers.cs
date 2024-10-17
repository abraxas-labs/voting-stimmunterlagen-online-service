// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddEventStateEventNumbers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "LatestEverProcessedEventPosition",
            table: "EventProcessingStates",
            newName: "LatestEverProcessedEventNumber");

        migrationBuilder.RenameColumn(
            name: "LastProcessedEventPosition",
            table: "EventProcessingStates",
            newName: "LastProcessedEventPreparePosition");

        migrationBuilder.AddColumn<decimal>(
            name: "LastProcessedEventCommitPosition",
            table: "EventProcessingStates",
            type: "numeric(20,0)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "LastProcessedEventNumber",
            table: "EventProcessingStates",
            type: "numeric(20,0)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastProcessedEventCommitPosition",
            table: "EventProcessingStates");

        migrationBuilder.DropColumn(
            name: "LastProcessedEventNumber",
            table: "EventProcessingStates");

        migrationBuilder.RenameColumn(
            name: "LatestEverProcessedEventNumber",
            table: "EventProcessingStates",
            newName: "LatestEverProcessedEventPosition");

        migrationBuilder.RenameColumn(
            name: "LastProcessedEventPreparePosition",
            table: "EventProcessingStates",
            newName: "LastProcessedEventPosition");
    }
}
