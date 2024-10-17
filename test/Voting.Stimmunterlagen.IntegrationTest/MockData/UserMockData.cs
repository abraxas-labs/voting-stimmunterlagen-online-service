// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class UserMockData
{
    public static User GemeindepraesidentArnegg => new()
    {
        FirstName = "Ruedi",
        LastName = "von der Arnegg",
        UserName = "UXRA0",
        SecureConnectId = "50897bca-86c2-4a65-a01e-c620baafa323",
    };

    public static User StadtpraesidentUzwil => new()
    {
        FirstName = "Hans-Peter",
        LastName = "von Uzwil",
        UserName = "UXHPU0",
        SecureConnectId = "fbfe3459-58b7-4f77-b8cb-9be85cc624e2",
    };

    public static User DruckverwalterUtz => new()
    {
        FirstName = "Utz",
        LastName = "van Druckverwalter",
        UserName = "AXDVZUTZ",
        SecureConnectId = "f8c0407a-255f-471b-ab15-79ca06c53283",
    };
}
