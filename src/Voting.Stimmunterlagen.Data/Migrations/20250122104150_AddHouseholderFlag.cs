// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddHouseholderFlag : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsHouseholder",
            table: "Voters",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "NumberOfHouseholders",
            table: "VoterLists",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "SendOnlyToHouseholder",
            table: "Attachments",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(@"
                UPDATE ""Voters""
                SET ""IsHouseholder"" = true
            ");

        migrationBuilder.Sql(@"
                UPDATE ""VoterLists""
                SET ""NumberOfHouseholders"" = ""NumberOfVoters""
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsHouseholder",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "NumberOfHouseholders",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "SendOnlyToHouseholder",
            table: "Attachments");
    }
}
