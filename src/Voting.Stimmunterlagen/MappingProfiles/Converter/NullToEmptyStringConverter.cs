// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;

namespace Voting.Stimmunterlagen.MappingProfiles.Converter;

public class NullToEmptyStringConverter : ITypeConverter<string?, string>
{
    public string Convert(string? source, string destination, ResolutionContext context)
        => source ?? string.Empty;
}
