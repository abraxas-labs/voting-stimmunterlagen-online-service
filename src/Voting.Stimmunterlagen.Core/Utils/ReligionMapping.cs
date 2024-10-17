// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Core.Utils;

public static class ReligionMapping
{
    public static string GetReligiousDenomination(string? religionCode)
    {
        // according eCH-0011_8_1 STAN pdf
        return religionCode switch
        {
            null => "Unbekannt",
            "000" => "Unbekannt",
            "111" => "evangelisch-reformierte (protestantische) Kirche",
            "121" => "römisch-katholische Kirche",
            "122" => "christkatholische / altkatholische Kirche",
            "211" => "israelitische Gemeinschaft / jüdische Glaubensgemeinschaft",
            "211201" => "Israelitische Cultusgemeinde",
            "211301" => "Jüdisch Liberale Gemeinde",
            _ => "Unbekannt",
        };
    }
}
