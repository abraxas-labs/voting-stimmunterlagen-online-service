// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVoterListImport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VoterListImports",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DomainOfInfluenceId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceId = table.Column<string>(type: "text", nullable: false),
                Source = table.Column<int>(type: "integer", nullable: false),
                LastUpdate = table.Column<DateTime>(type: "date", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterListImports", x => x.Id);
                table.ForeignKey(
                    name: "FK_VoterListImports_ContestDomainOfInfluences_DomainOfInfluenc~",
                    column: x => x.DomainOfInfluenceId,
                    principalTable: "ContestDomainOfInfluences",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(@"ALTER TABLE ""VoterLists"" DROP CONSTRAINT CK_VOTERLISTS_SOURCE");
        migrationBuilder.Sql(@"ALTER TABLE ""VoterListImports"" ADD CONSTRAINT CK_VOTERLIST_IMPORTS_SOURCE CHECK (""Source"" >= 1 AND ""Source"" <= 2)");

        migrationBuilder.AddColumn<Guid>(
            name: "ImportId",
            table: "VoterLists",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<bool>(
            name: "SendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "Voters",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        // add a voter list import for each voter list
        migrationBuilder.Sql(@$"
            INSERT INTO ""VoterListImports"" (""Id"", ""DomainOfInfluenceId"", ""Source"", ""SourceId"", ""LastUpdate"", ""Name"")
            SELECT ""Id"", ""DomainOfInfluenceId"", ""Source"", ""SourceId"", ""LastUpdate"", ""Name""
            FROM ""VoterLists"";

            UPDATE ""VoterLists""
            SET ""ImportId"" = ""Id""
        ");

        // set send voting cards to doi return address on each voter
        migrationBuilder.Sql(@$"
            UPDATE ""Voters"" v
            SET ""SendVotingCardsToDomainOfInfluenceReturnAddress"" = vl.""SendVotingCardsToDomainOfInfluenceReturnAddress""
            FROM ""VoterLists"" vl
            WHERE v.""ListId"" = vl.""Id""
        ");

        migrationBuilder.RenameColumn(
            name: "Source",
            table: "VoterLists",
            newName: "CountOfSendVotingCardsToDomainOfInfluenceReturnAddress");

        // set count of send voting cards to doi return address on voter lists
        migrationBuilder.Sql($@"
            UPDATE ""VoterLists""
            SET ""CountOfSendVotingCardsToDomainOfInfluenceReturnAddress"" = 0;

            UPDATE ""VoterLists""
            SET ""CountOfSendVotingCardsToDomainOfInfluenceReturnAddress"" = ""NumberOfVoters""
            WHERE ""SendVotingCardsToDomainOfInfluenceReturnAddress"" = TRUE
        ");

        migrationBuilder.DropColumn(
            name: "LastUpdate",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "Name",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "SourceId",
            table: "VoterLists");

        migrationBuilder.AlterColumn<bool>(
            name: "SendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "VoterLists",
            type: "boolean",
            nullable: true,
            oldClrType: typeof(bool),
            oldType: "boolean");

        migrationBuilder.AddColumn<bool>(
            name: "HasVoterDuplicates",
            table: "VoterLists",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "VoterDuplicates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                PersonId = table.Column<string>(type: "text", nullable: false),
                DateOfBirth = table.Column<string>(type: "text", nullable: false),
                Sex = table.Column<int>(type: "integer", nullable: false),
                ListId = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterDuplicates", x => x.Id);
                table.ForeignKey(
                    name: "FK_VoterDuplicates_VoterLists_ListId",
                    column: x => x.ListId,
                    principalTable: "VoterLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists",
            columns: new[] { "ImportId", "VotingCardType" });

        migrationBuilder.CreateIndex(
            name: "IX_VoterDuplicates_ListId",
            table: "VoterDuplicates",
            column: "ListId");

        migrationBuilder.CreateIndex(
            name: "IX_VoterListImports_DomainOfInfluenceId",
            table: "VoterListImports",
            column: "DomainOfInfluenceId");

        migrationBuilder.AddForeignKey(
            name: "FK_VoterLists_VoterListImports_ImportId",
            table: "VoterLists",
            column: "ImportId",
            principalTable: "VoterListImports",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_VoterLists_VoterListImports_ImportId",
            table: "VoterLists");

        migrationBuilder.DropTable(
            name: "VoterDuplicates");

        migrationBuilder.DropTable(
            name: "VoterListImports");

        migrationBuilder.DropIndex(
            name: "IX_VoterLists_ImportId_VotingCardType",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "SendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "HasVoterDuplicates",
            table: "VoterLists");

        migrationBuilder.DropColumn(
            name: "ImportId",
            table: "VoterLists");

        migrationBuilder.RenameColumn(
            name: "CountOfSendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "VoterLists",
            newName: "Source");

        migrationBuilder.AlterColumn<bool>(
            name: "SendVotingCardsToDomainOfInfluenceReturnAddress",
            table: "VoterLists",
            type: "boolean",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "boolean",
            oldNullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastUpdate",
            table: "VoterLists",
            type: "date",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "VoterLists",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "SourceId",
            table: "VoterLists",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }
}
