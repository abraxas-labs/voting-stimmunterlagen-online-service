// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class FileNameUtils
{
    private static readonly char[] InvalidPathChars = Path.GetInvalidFileNameChars();

    public static string SanitizeFileName(string fileName)
    {
        return string.Join(
            "_",
            fileName.Split(InvalidPathChars, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string GenerateFileName(string fileNamePattern, IReadOnlyCollection<string>? fileNameArgs)
    {
        if (fileNameArgs == null || fileNameArgs.Count == 0)
        {
            return SanitizeFileName(fileNamePattern);
        }

        var fileName = string.Format(CultureInfo.InvariantCulture, fileNamePattern, fileNameArgs.OfType<object?>().ToArray());
        return SanitizeFileName(fileName);
    }
}
