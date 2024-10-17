// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVoterPageInfo : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PageInfo_PageFrom",
            table: "Voters",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PageInfo_PageTo",
            table: "Voters",
            type: "integer",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PageInfo_PageFrom",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "PageInfo_PageTo",
            table: "Voters");
    }
}
