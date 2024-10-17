// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Utils;

public static class DatamatrixMapping
{
    public const int PersonIdLength = 11;

    public static string MapContestOrderNumber(int orderNumber)
        => orderNumber.ToString().PadLeft(6, '0');

    public static string MapVoterShipmentNumber(int shipmentNumber)
        => shipmentNumber.ToString().PadLeft(9, '0');

    public static string MapPersonId(string personId)
        => personId.PadLeft(PersonIdLength, '0');
}
