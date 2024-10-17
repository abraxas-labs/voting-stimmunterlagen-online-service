// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class RemoveVotingCardGroupBfs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE ""DomainOfInfluenceVotingCardConfigurations""
            SET ""Groups"" = array_remove(""Groups"", 1);
        ");

        migrationBuilder.Sql(@"
            UPDATE ""DomainOfInfluenceVotingCardConfigurations""
            SET ""Groups"" = array_replace(""Groups"", 2, 1);
        ");

        migrationBuilder.Sql(@"
            UPDATE ""DomainOfInfluenceVotingCardConfigurations""
            SET ""Groups"" = array_replace(""Groups"", 3, 2);
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
