// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddGinIndices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("create extension if not exists pg_trgm");

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluences_AuthorityName",
            table: "ContestDomainOfInfluences",
            column: "AuthorityName")
            .Annotation("Npgsql:IndexMethod", "gin")
            .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_Name",
            table: "Attachments",
            column: "Name")
            .Annotation("Npgsql:IndexMethod", "gin")
            .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_ContestDomainOfInfluences_AuthorityName",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropIndex(
            name: "IX_Attachments_Name",
            table: "Attachments");

        migrationBuilder.Sql("drop extension pg_trgm");
    }
}
