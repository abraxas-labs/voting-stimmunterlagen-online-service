// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0045_4_0;
using Voting.Lib.Common;

namespace Voting.Stimmunterlagen.Ech.Mapping.V4;

internal static class LanguageMapping
{
    public static string ToLanguage(this LanguageType languageType)
    {
        return languageType switch
        {
            LanguageType.De => Languages.German,
            LanguageType.It => Languages.Italian,
            LanguageType.Fr => Languages.French,
            LanguageType.Rm => Languages.Romansh,
            _ => throw new ArgumentException($"invalid language type {languageType}"),
        };
    }

    public static LanguageType ToEchLanguage(this string language)
    {
        return language switch
        {
            Languages.German => LanguageType.De,
            Languages.Italian => LanguageType.It,
            Languages.French => LanguageType.Fr,
            Languages.Romansh => LanguageType.Rm,
            _ => throw new ArgumentException($"invalid language {language}"),
        };
    }
}
