// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddContestGenerateVotingCardsDeadline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "GenerateVotingCardsDeadline",
            table: "Contests",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.Sql(@$"
                UPDATE ""Contests""
                SET ""GenerateVotingCardsDeadline"" = ""PrintingCenterSignUpDeadline""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GenerateVotingCardsDeadline",
            table: "Contests");
    }
}
