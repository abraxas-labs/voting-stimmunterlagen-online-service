// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVoterListSourceAndSourceId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "FileName",
            table: "VoterLists",
            newName: "SourceId");

        migrationBuilder.AddColumn<int>(
            name: "Source",
            table: "VoterLists",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql(@"UPDATE ""VoterLists"" SET ""Source"" = 1");
        migrationBuilder.Sql(@"ALTER TABLE ""VoterLists"" ADD CONSTRAINT CK_VOTERLISTS_SOURCE CHECK (""Source"" >= 1 AND ""Source"" <= 2)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Source",
            table: "VoterLists");

        migrationBuilder.RenameColumn(
            name: "SourceId",
            table: "VoterLists",
            newName: "FileName");
    }
}
