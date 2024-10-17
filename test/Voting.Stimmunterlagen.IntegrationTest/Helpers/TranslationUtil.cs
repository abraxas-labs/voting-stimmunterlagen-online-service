// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public static class TranslationUtil
{
    public static List<T> CreateTranslations<T>(
        Action<T, string> field1Setter,
        string field1Value,
        Action<T, string>? field2Setter = null,
        string? field2Value = null)
        where T : TranslationEntity, new()
    {
        var translations = new List<T>();

        foreach (var (lang, field1Content) in LanguageUtil.MockAllLanguages(field1Value))
        {
            var translation = new T { Language = lang };
            field1Setter(translation, field1Content);

            if (field2Setter != null)
            {
                var field2Content = $"{field2Value} {lang}";
                field2Setter(translation, field2Content);
            }

            translations.Add(translation);
        }

        return translations;
    }
}
