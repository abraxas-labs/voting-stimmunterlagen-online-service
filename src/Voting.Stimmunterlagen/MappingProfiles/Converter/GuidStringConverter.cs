// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using AutoMapper;
using Voting.Lib.Common;

namespace Voting.Stimmunterlagen.MappingProfiles.Converter;

public class GuidStringConverter :
    ITypeConverter<Guid?, string>,
    ITypeConverter<string, Guid?>
{
    public string Convert(Guid? source, string destination, ResolutionContext context)
        => source == null ? string.Empty : source.Value.ToString();

    public Guid? Convert(string source, Guid? destination, ResolutionContext context)
        => GuidParser.ParseNullable(source);
}
