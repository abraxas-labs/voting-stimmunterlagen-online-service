// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RemoveUnusedAttachmentsAndVoterLists : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM ""Attachments"" a
            WHERE NOT EXISTS (
                SELECT 1 FROM ""PoliticalBusinessAttachmentEntries"" x
                WHERE x.""AttachmentId"" = a.""Id""
            )
            OR NOT EXISTS (
                SELECT 1 FROM ""StepStates"" s
                WHERE s.""Step"" = 6 AND s.""DomainOfInfluenceId"" = a.""DomainOfInfluenceId""
            )");

        migrationBuilder.Sql(@"
            DELETE FROM ""VoterLists"" v
            WHERE NOT EXISTS (
                SELECT 1 FROM ""PoliticalBusinessVoterListEntries"" x
                WHERE v.""Id"" = x.""VoterListId""
            )
            OR NOT EXISTS (
                SELECT 1 FROM ""StepStates"" s
                WHERE s.""Step"" = 7 AND s.""DomainOfInfluenceId"" = v.""DomainOfInfluenceId""
            )");
    }
}
