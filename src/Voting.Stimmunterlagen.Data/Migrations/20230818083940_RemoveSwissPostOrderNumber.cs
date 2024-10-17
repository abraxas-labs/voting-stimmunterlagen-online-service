﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class RemoveSwissPostOrderNumber : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SwissPostData_OrderNumber",
            table: "DomainOfInfluences");

        migrationBuilder.DropColumn(
            name: "SwissPostData_OrderNumber",
            table: "ContestDomainOfInfluences");

        migrationBuilder.RenameColumn(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "DomainOfInfluences",
            newName: "SwissPostData_FrankingLicenceReturnNumber");

        migrationBuilder.RenameColumn(
            name: "SwissPostData_FrankingLicenceNumber",
            table: "ContestDomainOfInfluences",
            newName: "SwissPostData_FrankingLicenceReturnNumber");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SwissPostData_FrankingLicenceReturnNumber",
            table: "DomainOfInfluences",
            newName: "SwissPostData_FrankingLicenceNumber");

        migrationBuilder.RenameColumn(
            name: "SwissPostData_FrankingLicenceReturnNumber",
            table: "ContestDomainOfInfluences",
            newName: "SwissPostData_FrankingLicenceNumber");

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_OrderNumber",
            table: "DomainOfInfluences",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SwissPostData_OrderNumber",
            table: "ContestDomainOfInfluences",
            type: "text",
            nullable: true);
    }
}
