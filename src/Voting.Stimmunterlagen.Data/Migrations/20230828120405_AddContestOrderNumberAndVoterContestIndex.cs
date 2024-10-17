// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddContestOrderNumberAndVoterContestIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ContestId",
            table: "Voters",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<int>(
            name: "ContestIndex",
            table: "Voters",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "OrderNumber",
            table: "Contests",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "ContestOrderNumberStates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LastSetOrderNumber = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContestOrderNumberStates", x => x.Id);
            });

        // set contest order number
        migrationBuilder.Sql(@"
CREATE SEQUENCE tmp_contest_order_number MINVALUE 999000;
UPDATE ""Contests"" SET ""OrderNumber"" = nextval('tmp_contest_order_number');
DROP SEQUENCE tmp_contest_order_number;
            ");

        // create sequences for voter contest index.
        migrationBuilder.Sql(@"
DO $$ DECLARE
    r TEXT;
BEGIN
    FOR r IN (
        SELECT ""Id""
        FROM ""Contests""
    ) LOOP
        EXECUTE 'CREATE SEQUENCE ""VoterContestIndex_' || r || '"" MAXVALUE 999999999';
    END LOOP;
END $$;
            ");

        // update voters from voter lists.
        migrationBuilder.Sql(@"
UPDATE ""Voters"" v1
SET ""ContestId"" = cd.""ContestId"",
    ""ContestIndex"" = nextval('""VoterContestIndex_' || cd.""ContestId"" || '""')
FROM ""Voters"" v2
JOIN ""VoterLists"" vl ON v2.""ListId"" = vl.""Id""
JOIN ""ContestDomainOfInfluences"" cd ON vl.""DomainOfInfluenceId"" = cd.""Id""
WHERE v1.""Id"" = v2.""Id""
            ");

        // update voters from manual jobs.
        migrationBuilder.Sql(@"
UPDATE ""Voters"" v1
SET ""ContestId"" = cd.""ContestId"",
    ""ContestIndex"" = nextval('""VoterContestIndex_' || cd.""ContestId"" || '""')
FROM ""Voters"" v2
JOIN ""ManualVotingCardGeneratorJobs"" mj ON v2.""ManualJobId"" = mj.""Id""
JOIN ""DomainOfInfluenceVotingCardLayouts"" dl ON mj.""LayoutId"" = dl.""Id""
JOIN ""ContestDomainOfInfluences"" cd ON dl.""DomainOfInfluenceId"" = cd.""Id""
WHERE v1.""Id"" = v2.""Id""
            ");

        // create trigger and function to update voter contest index.
        migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION set_voter_contest_index()
RETURNS trigger
AS $$
BEGIN
    NEW.""ContestIndex"" := nextval('""VoterContestIndex_' || NEW.""ContestId"" || '""');
    RETURN NEW;
    END;
$$
LANGUAGE PLPGSQL;

CREATE TRIGGER voter_contest_index_trigger
BEFORE INSERT ON ""Voters""
FOR EACH ROW EXECUTE FUNCTION set_voter_contest_index();
            ");

        migrationBuilder.CreateIndex(
            name: "IX_Voters_ContestId_ContestIndex",
            table: "Voters",
            columns: new[] { "ContestId", "ContestIndex" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Voters_Contests_ContestId",
            table: "Voters",
            column: "ContestId",
            principalTable: "Contests",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Voters_Contests_ContestId",
            table: "Voters");

        migrationBuilder.DropTable(
            name: "ContestOrderNumberStates");

        migrationBuilder.DropIndex(
            name: "IX_Voters_ContestId_ContestIndex",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "ContestId",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "ContestIndex",
            table: "Voters");

        migrationBuilder.DropColumn(
            name: "OrderNumber",
            table: "Contests");
    }
}
