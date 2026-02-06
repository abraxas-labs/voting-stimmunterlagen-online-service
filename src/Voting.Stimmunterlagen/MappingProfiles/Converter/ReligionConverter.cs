// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles.Converter;

public class ReligionConverter : IValueConverter<Religion, string?>
{
    public string? Convert(Religion source, ResolutionContext context)
    {
        return source switch
        {
            Religion.Unspecified => string.Empty,
            Religion.Other => "000",
            Religion.Protestant => "111",
            Religion.Catholic => "121",
            Religion.ProtestantMinorOrForeigner => "111",
            Religion.CatholicMinorOrForeigner => "121",
            _ => string.Empty,
        };
    }
}
