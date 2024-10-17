// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingCardColor : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "DomainOfInfluences",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "ContestDomainOfInfluences",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "ContestDomainOfInfluences");
    }
}
