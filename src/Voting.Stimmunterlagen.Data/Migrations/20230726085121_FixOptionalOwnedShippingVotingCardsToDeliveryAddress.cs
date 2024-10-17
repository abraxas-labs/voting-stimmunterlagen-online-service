﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.Stimmunterlagen.Data.Migrations;

public partial class FixOptionalOwnedShippingVotingCardsToDeliveryAddress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
                UPDATE ""DomainOfInfluences""
                SET ""PrintData_ShippingVotingCardsToDeliveryAddress""=false
                WHERE ""ResponsibleForVotingCards""=true AND ""PrintData_ShippingVotingCardsToDeliveryAddress"" IS NULL");

        migrationBuilder.Sql(@"
                UPDATE ""ContestDomainOfInfluences""
                SET ""PrintData_ShippingVotingCardsToDeliveryAddress""=false
                WHERE ""ResponsibleForVotingCards""=true AND ""PrintData_ShippingVotingCardsToDeliveryAddress"" IS NULL");
    }
}
