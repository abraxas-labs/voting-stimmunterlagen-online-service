// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class FixContestDomainOfInfluenceCanton : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE ""ContestDomainOfInfluences"" cdoi
            SET ""Canton"" = doi.""Canton"",
                ""CantonDefaults_ElectoralRegistrationEnabled"" = doi.""CantonDefaults_ElectoralRegistrationEnabled"",
                ""CantonDefaults_VotingDocumentsEVotingEaiMessageType"" = doi.""CantonDefaults_VotingDocumentsEVotingEaiMessageType""
            FROM ""DomainOfInfluences"" doi
            WHERE cdoi.""Canton"" = 0
            AND cdoi.""BasisDomainOfInfluenceId"" = doi.""Id""");
    }
}
