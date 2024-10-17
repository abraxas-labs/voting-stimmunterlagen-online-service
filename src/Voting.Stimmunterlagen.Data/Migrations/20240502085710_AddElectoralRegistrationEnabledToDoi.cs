// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddElectoralRegistrationEnabledToDoi : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ElectoralRegistrationEnabled",
            table: "CantonSettings");

        migrationBuilder.RenameColumn(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "DomainOfInfluences",
            newName: "ElectoralRegistrationEnabled");

        migrationBuilder.RenameColumn(
            name: "CantonDefaults_ElectoralRegistrationEnabled",
            table: "ContestDomainOfInfluences",
            newName: "ElectoralRegistrationEnabled");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "ElectoralRegistrationEnabled",
            table: "DomainOfInfluences",
            newName: "CantonDefaults_ElectoralRegistrationEnabled");

        migrationBuilder.RenameColumn(
            name: "ElectoralRegistrationEnabled",
            table: "ContestDomainOfInfluences",
            newName: "CantonDefaults_ElectoralRegistrationEnabled");

        migrationBuilder.AddColumn<bool>(
            name: "ElectoralRegistrationEnabled",
            table: "CantonSettings",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }
}
