// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddEVotingStep : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp""");

        migrationBuilder.DropIndex("IX_StepStates_DomainOfInfluenceId_Step");

        // new step "e-voting" is added with enum value 9 and shifts greater or equal enum values by 1.
        migrationBuilder.Sql(@"UPDATE ""StepStates"" SET ""Step"" = ""Step"" + 1 WHERE ""Step"" >= 9");

        migrationBuilder.CreateIndex(
            name: "IX_StepStates_DomainOfInfluenceId_Step",
            table: "StepStates",
            columns: new[] { "DomainOfInfluenceId", "Step" },
            unique: true);

        // add new step "e-voting" if contest has active e voting.
        migrationBuilder.Sql(@"
            INSERT INTO ""StepStates"" (""Id"", ""DomainOfInfluenceId"", ""Step"", ""Approved"")
            SELECT uuid_generate_v4(), ""DomainOfInfluenceId"", 9, false FROM ""Contests""
            WHERE ""EVoting"" = true");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
