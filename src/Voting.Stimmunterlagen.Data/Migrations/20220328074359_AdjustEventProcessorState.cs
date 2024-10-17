// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AdjustEventProcessorState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastProcessedEventNumber",
            table: "EventProcessingStates");

        migrationBuilder.DropColumn(
            name: "LatestEverProcessedEventNumber",
            table: "EventProcessingStates");

        migrationBuilder.AddColumn<decimal>(
            name: "LastProcessedEventPosition",
            table: "EventProcessingStates",
            type: "numeric(20,0)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "LatestEverProcessedEventPosition",
            table: "EventProcessingStates",
            type: "numeric(20,0)",
            nullable: false,
            defaultValue: 0m);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastProcessedEventPosition",
            table: "EventProcessingStates");

        migrationBuilder.DropColumn(
            name: "LatestEverProcessedEventPosition",
            table: "EventProcessingStates");

        migrationBuilder.AddColumn<long>(
            name: "LastProcessedEventNumber",
            table: "EventProcessingStates",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "LatestEverProcessedEventNumber",
            table: "EventProcessingStates",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);
    }
}
