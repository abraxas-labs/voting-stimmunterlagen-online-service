// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingCardsShipmentWeight : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "VotingCardsPackedCount",
            table: "PrintJobs");

        migrationBuilder.RenameColumn(
            name: "VotingCardsPrintedCount",
            table: "PrintJobs",
            newName: "VotingCardsPrintedAndPackedCount");

        migrationBuilder.AddColumn<double>(
            name: "VotingCardsShipmentWeight",
            table: "PrintJobs",
            type: "double precision",
            nullable: false,
            defaultValue: 0.0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "VotingCardsShipmentWeight",
            table: "PrintJobs");

        migrationBuilder.RenameColumn(
            name: "VotingCardsPrintedAndPackedCount",
            table: "PrintJobs",
            newName: "VotingCardsPrintedCount");

        migrationBuilder.AddColumn<int>(
            name: "VotingCardsPackedCount",
            table: "PrintJobs",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
