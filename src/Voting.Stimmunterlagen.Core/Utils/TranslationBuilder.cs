// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class TranslationBuilder
{
    public static List<T> CreateTranslations<T>(
        Action<T, string> field1Setter,
        IDictionary<string, string> field1Values,
        Action<T, string>? field2Setter = null,
        IDictionary<string, string>? field2Values = null)
        where T : TranslationEntity, new()
    {
        var translations = new List<T>();
        var languages = field1Values
            .Select(x => x.Key);

        if (field2Values != null)
        {
            languages = languages
                .Concat(field2Values.Select(x => x.Key))
                .Distinct();
        }

        foreach (var lang in languages)
        {
            var translation = new T { Language = lang };

            if (field1Values.TryGetValue(lang, out var field1Content))
            {
                field1Setter(translation, field1Content);
            }

            if (field2Setter != null && field2Values != null && field2Values.TryGetValue(lang, out var field2Content))
            {
                field2Setter(translation, field2Content);
            }

            translations.Add(translation);
        }

        return translations;
    }
}
