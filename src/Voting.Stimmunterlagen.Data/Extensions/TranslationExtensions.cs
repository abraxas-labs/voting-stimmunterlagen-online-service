// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace Voting.Stimmunterlagen.Data.Extensions;

public static class TranslationExtensions
{
    public static string GetTranslated<T>(this IEnumerable<T>? translations, Func<T, string> fieldFunc)
    {
        if (translations == null)
        {
            throw new InvalidOperationException("Translations not loaded");
        }

        if (translations.Count() > 1)
        {
            throw new InvalidOperationException("Multiple translations available. Correct result cannot be guaranteed");
        }

        var translation = translations.FirstOrDefault()
            ?? throw new InvalidOperationException("Translations not available for the current language");

        return fieldFunc(translation);
    }
}
