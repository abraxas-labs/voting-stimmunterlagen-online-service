// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVoterDuplicateStreet : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "HouseNumber",
            table: "DomainOfInfluenceVoterDuplicates",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Street",
            table: "DomainOfInfluenceVoterDuplicates",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HouseNumber",
            table: "DomainOfInfluenceVoterDuplicates");

        migrationBuilder.DropColumn(
            name: "Street",
            table: "DomainOfInfluenceVoterDuplicates");
    }
}
