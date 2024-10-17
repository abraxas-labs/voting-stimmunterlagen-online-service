// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class AddPlaceOfOrigin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VoterPlaceOfOrigin",
            columns: table => new
            {
                VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Canton = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VoterPlaceOfOrigin", x => new { x.VoterId, x.Id });
                table.ForeignKey(
                    name: "FK_VoterPlaceOfOrigin_Voters_VoterId",
                    column: x => x.VoterId,
                    principalTable: "Voters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VoterPlaceOfOrigin");
    }
}
