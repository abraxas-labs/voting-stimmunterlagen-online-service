// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddDomainOfInfluenceCantonDefaults : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "DomainOfInfluences",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<Guid>(
            name: "RootId",
            table: "DomainOfInfluences",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "ContestDomainOfInfluences",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<Guid>(
            name: "RootId",
            table: "ContestDomainOfInfluences",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CantonDefaults_VotingDocumentsEVotingEaiMessageType",
            table: "DomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "CantonDefaults_VotingDocumentsEVotingEaiMessageType",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_DomainOfInfluences_RootId",
            table: "DomainOfInfluences",
            column: "RootId");

        migrationBuilder.CreateIndex(
            name: "IX_ContestDomainOfInfluences_RootId",
            table: "ContestDomainOfInfluences",
            column: "RootId");

        migrationBuilder.AddForeignKey(
            name: "FK_ContestDomainOfInfluences_ContestDomainOfInfluences_RootId",
            table: "ContestDomainOfInfluences",
            column: "RootId",
            principalTable: "ContestDomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_DomainOfInfluences_DomainOfInfluences_RootId",
            table: "DomainOfInfluences",
            column: "RootId",
            principalTable: "DomainOfInfluences",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        // set root ids and set root id columns non-null
        migrationBuilder.Sql(@"
            UPDATE ""DomainOfInfluences""
            SET ""RootId"" = t.RootId, ""Canton"" = t.RootCanton
                FROM (WITH RECURSIVE child_or_self AS (SELECT ""Id"", ""ParentId"", ""Id"" AS RootId, ""Canton"" as RootCanton, 1 AS level
                FROM ""DomainOfInfluences""
                WHERE ""ParentId"" IS NULL
                UNION ALL
                SELECT doi.""Id"", doi.""ParentId"", child_or_self.RootId, child_or_self.RootCanton, child_or_self.level + 1
                FROM ""DomainOfInfluences"" doi
                JOIN child_or_self ON doi.""ParentId"" = child_or_self.""Id"")
                SELECT ""Id"", RootId, RootCanton
                FROM child_or_self) as t
                WHERE t.""Id"" = ""DomainOfInfluences"".""Id"";

            UPDATE ""DomainOfInfluences""
            SET ""CantonDefaults_VotingDocumentsEVotingEaiMessageType"" = t.""VotingDocumentsEVotingEaiMessageType""
            FROM (SELECT ""Canton"", ""VotingDocumentsEVotingEaiMessageType""
                  FROM ""CantonSettings"") as t
            WHERE t.""Canton"" = ""DomainOfInfluences"".""Canton"";

            ALTER TABLE ""DomainOfInfluences"" ALTER COLUMN ""RootId"" SET NOT NULL;

            UPDATE ""ContestDomainOfInfluences""
            SET ""RootId"" = t.RootId, ""Canton"" = t.RootCanton
                FROM (WITH RECURSIVE child_or_self AS (SELECT ""Id"", ""ParentId"", ""Id"" AS RootId, ""Canton"" as RootCanton, 1 AS level
                FROM ""ContestDomainOfInfluences""
                WHERE ""ParentId"" IS NULL
                UNION ALL
                SELECT doi.""Id"", doi.""ParentId"", child_or_self.RootId, child_or_self.RootCanton, child_or_self.level + 1
                FROM ""ContestDomainOfInfluences"" doi
                JOIN child_or_self ON doi.""ParentId"" = child_or_self.""Id"")
                SELECT ""Id"", RootId, RootCanton
                FROM child_or_self) as t
                WHERE t.""Id"" = ""ContestDomainOfInfluences"".""Id"";

            UPDATE ""ContestDomainOfInfluences""
            SET ""CantonDefaults_VotingDocumentsEVotingEaiMessageType"" = t.""VotingDocumentsEVotingEaiMessageType""
            FROM (SELECT ""Canton"", ""VotingDocumentsEVotingEaiMessageType""
                  FROM ""CantonSettings"") as t
            WHERE t.""Canton"" = ""ContestDomainOfInfluences"".""Canton"";

            ALTER TABLE ""ContestDomainOfInfluences"" ALTER COLUMN ""RootId"" SET NOT NULL;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ContestDomainOfInfluences_ContestDomainOfInfluences_RootId",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropForeignKey(
            name: "FK_DomainOfInfluences_DomainOfInfluences_RootId",
            table: "DomainOfInfluences");

        migrationBuilder.DropIndex(
            name: "IX_DomainOfInfluences_RootId",
            table: "DomainOfInfluences");

        migrationBuilder.DropIndex(
            name: "IX_ContestDomainOfInfluences_RootId",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "RootId",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "RootId",
            table: "ContestDomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "CantonDefaults_VotingDocumentsEVotingEaiMessageType",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "CantonDefaults_VotingDocumentsEVotingEaiMessageType",
            table: "ContestDomainOfInfluences");
    }
}
