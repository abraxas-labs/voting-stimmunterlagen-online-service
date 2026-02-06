// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Ech.Mapping;

public static class PostOfficeBoxMapping
{
    public static string? AddPostOfficeBoxNumber(this string postOfficeBoxText, uint? postOfficeBoxNumber)
    {
        return postOfficeBoxText == null ? null : $"{postOfficeBoxText} {postOfficeBoxNumber?.ToString() ?? string.Empty}".Trim();
    }
}
