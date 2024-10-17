// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddVotingJournal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LastVoterUpdate",
            table: "ContestDomainOfInfluences",
            type: "timestamp with time zone",
            nullable: true);

        // add new step "voting-journal" if a domain of influence has the step "voter-lists".
        migrationBuilder.Sql(@"
                INSERT INTO ""StepStates"" (""Id"", ""DomainOfInfluenceId"", ""Step"", ""Approved"")
                SELECT uuid_generate_v4(), ""DomainOfInfluenceId"", 13, false FROM ""StepStates""
                WHERE ""Step"" = 7");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastVoterUpdate",
            table: "ContestDomainOfInfluences");
    }
}
