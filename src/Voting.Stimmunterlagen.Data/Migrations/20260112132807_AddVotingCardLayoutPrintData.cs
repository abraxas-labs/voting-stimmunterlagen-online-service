// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddVotingCardLayoutPrintData : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingAway",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingMethod",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingReturn",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "PrintData_ShippingVotingCardsToDeliveryAddress",
            table: "DomainOfInfluenceVotingCardLayouts",
            type: "boolean",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingAway",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingMethod",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PrintData_ShippingReturn",
            table: "ContestVotingCardLayouts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "PrintData_ShippingVotingCardsToDeliveryAddress",
            table: "ContestVotingCardLayouts",
            type: "boolean",
            nullable: true);

        migrationBuilder.Sql("UPDATE \"ContestVotingCardLayouts\" cvcl SET \"PrintData_ShippingAway\" = doi.\"PrintData_ShippingAway\", \"PrintData_ShippingMethod\" = doi.\"PrintData_ShippingMethod\", \"PrintData_ShippingReturn\" = doi.\"PrintData_ShippingReturn\", \"PrintData_ShippingVotingCardsToDeliveryAddress\" = doi.\"PrintData_ShippingVotingCardsToDeliveryAddress\" FROM \"Contests\" c JOIN \"ContestDomainOfInfluences\" cdi ON cdi.\"Id\" = c.\"DomainOfInfluenceId\" JOIN \"DomainOfInfluences\" doi ON doi.\"Id\" = cdi.\"BasisDomainOfInfluenceId\" WHERE cvcl.\"ContestId\" = c.\"Id\"");

        migrationBuilder.Sql("UPDATE \"DomainOfInfluenceVotingCardLayouts\" doivcl SET \"PrintData_ShippingAway\" = doi.\"PrintData_ShippingAway\", \"PrintData_ShippingMethod\" = doi.\"PrintData_ShippingMethod\", \"PrintData_ShippingReturn\" = doi.\"PrintData_ShippingReturn\", \"PrintData_ShippingVotingCardsToDeliveryAddress\" = doi.\"PrintData_ShippingVotingCardsToDeliveryAddress\" FROM \"ContestDomainOfInfluences\" cdi JOIN \"DomainOfInfluences\" doi ON doi.\"Id\" = cdi.\"BasisDomainOfInfluenceId\" JOIN \"Contests\" c ON c.\"Id\" = cdi.\"ContestId\" WHERE doivcl.\"DomainOfInfluenceId\" = cdi.\"Id\" AND (cdi.\"ResponsibleForVotingCards\" AND EXISTS (SELECT 1 FROM \"PoliticalBusinessPermissions\" pbp WHERE pbp.\"DomainOfInfluenceId\" = cdi.\"Id\" AND pbp.\"Role\" = 1) OR (c.\"IsPoliticalAssembly\" AND cdi.\"Role\" != 0))");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PrintData_ShippingAway",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingMethod",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingReturn",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingVotingCardsToDeliveryAddress",
            table: "DomainOfInfluenceVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingAway",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingMethod",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingReturn",
            table: "ContestVotingCardLayouts");

        migrationBuilder.DropColumn(
            name: "PrintData_ShippingVotingCardsToDeliveryAddress",
            table: "ContestVotingCardLayouts");
    }
}
