// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class DummyDoiBuilder
{
    public static ContestDomainOfInfluence GetDummyDomainOfInfluence(string tennantId, DomainOfInfluenceVotingCardPrintData? printData, Data.Models.VotingCardColor color)
    {
        return new ContestDomainOfInfluence()
        {
            Name = "Test-Gemeinde XY",
            ShortName = "XY",
            SecureConnectId = tennantId,
            ReturnAddress = new() { AddressLine1 = "Gemeindeverwaltung XY", AddressLine2 = "Adresszeile 2", Street = "Strasse 99", AddressAddition = "Adress Zusatz", ZipCode = "9999", City = "XY", Country = "SWITZERLAND" },
            PrintData = printData,
            SwissPostData = new() { InvoiceReferenceNumber = "000000000", FrankingLicenceReturnNumber = "000000000" },
            LogoRef = string.Empty,
            VotingCardColor = color,
        };
    }
}
