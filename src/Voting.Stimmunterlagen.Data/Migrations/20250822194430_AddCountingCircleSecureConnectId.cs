// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddCountingCircleSecureConnectId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SecureConnectId",
            table: "CountingCircles",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "SecureConnectId",
            table: "ContestCountingCircles",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SecureConnectId",
            table: "CountingCircles");

        migrationBuilder.DropColumn(
            name: "SecureConnectId",
            table: "ContestCountingCircles");
    }
}
