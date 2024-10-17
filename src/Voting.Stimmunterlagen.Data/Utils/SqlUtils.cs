// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Utils;

public static class SqlUtils
{
    public const string DefaultEscapeCharacter = "$";

    public static string EscapeLike(string pattern, string escapeChar = DefaultEscapeCharacter)
    {
        return pattern
            .Replace(escapeChar, $"{escapeChar}{escapeChar}", StringComparison.OrdinalIgnoreCase)
            .Replace("%", escapeChar + "%", StringComparison.OrdinalIgnoreCase)
            .Replace("_", escapeChar + "_", StringComparison.OrdinalIgnoreCase);
    }
}
