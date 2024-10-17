// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVoterListImportAutoSplit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit",
            table: "VoterListImports",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(@"
            UPDATE ""VoterListImports""
            SET ""AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit"" = true
            WHERE ""Source"" = 2
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit",
            table: "VoterListImports");
    }
}
