// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddDoiLatestImportsLastUpdate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastVoterUpdate",
            table: "ContestDomainOfInfluences");

        migrationBuilder.AddColumn<DateTime>(
            name: "LatestVoterListImportsLastUpdate",
            table: "ContestDomainOfInfluences",
            type: "date",
            nullable: true);

        migrationBuilder.Sql($@"
UPDATE ""ContestDomainOfInfluences"" cdoi
SET ""LatestVoterListImportsLastUpdate"" = v.""MaxLastUpdate""
FROM (
    SELECT ""DomainOfInfluenceId"", MAX(""LastUpdate"") AS ""MaxLastUpdate""
    FROM ""VoterListImports""
    GROUP BY ""DomainOfInfluenceId""
) v
WHERE cdoi.""Id"" = v.""DomainOfInfluenceId""
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LatestVoterListImportsLastUpdate",
            table: "ContestDomainOfInfluences");

        migrationBuilder.AddColumn<DateTime>(
            name: "LastVoterUpdate",
            table: "ContestDomainOfInfluences",
            type: "timestamp with time zone",
            nullable: true);
    }
}
