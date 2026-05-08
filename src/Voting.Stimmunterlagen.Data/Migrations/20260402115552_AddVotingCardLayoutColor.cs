// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVotingCardLayoutColor : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "DomainOfInfluenceVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "OverriddenVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "VotingCardColor",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql("UPDATE \"ContestVotingCardLayouts\" cvcl SET \"VotingCardColor\" = doi.\"VotingCardColor\" FROM \"Contests\" c JOIN \"ContestDomainOfInfluences\" cdi ON cdi.\"Id\" = c.\"DomainOfInfluenceId\" JOIN \"DomainOfInfluences\" doi ON doi.\"Id\" = cdi.\"BasisDomainOfInfluenceId\" WHERE cvcl.\"ContestId\" = c.\"Id\"");

        migrationBuilder.Sql("UPDATE \"DomainOfInfluenceVotingCardLayouts\" doivcl SET \"VotingCardColor\" = doi.\"VotingCardColor\", \"DomainOfInfluenceVotingCardColor\" = doi.\"VotingCardColor\" FROM \"ContestDomainOfInfluences\" cdi JOIN \"DomainOfInfluences\" doi ON doi.\"Id\" = cdi.\"BasisDomainOfInfluenceId\" JOIN \"Contests\" c ON c.\"Id\" = cdi.\"ContestId\" WHERE doivcl.\"DomainOfInfluenceId\" = cdi.\"Id\" AND cdi.\"ResponsibleForVotingCards\"");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DomainOfInfluenceVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "OverriddenVotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "VotingCardColor",
            table: "ContestVotingCardLayouts");
    }
}
