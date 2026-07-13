// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

/// <inheritdoc />
public partial class AddAttachmentDelayedDeliveryDate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "DelayedDeliveryDate",
            table: "Attachments",
            type: "date",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DelayedDeliveryDate",
            table: "Attachments");
    }
}
