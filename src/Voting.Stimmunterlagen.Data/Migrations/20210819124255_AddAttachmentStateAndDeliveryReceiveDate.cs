// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddAttachmentStateAndDeliveryReceiveDate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "DeliveryReceivedOn",
            table: "Attachments",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "State",
            table: "Attachments",
            type: "integer",
            nullable: false,
            defaultValue: 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DeliveryReceivedOn",
            table: "Attachments");

        migrationBuilder.DropColumn(
            name: "State",
            table: "Attachments");
    }
}
