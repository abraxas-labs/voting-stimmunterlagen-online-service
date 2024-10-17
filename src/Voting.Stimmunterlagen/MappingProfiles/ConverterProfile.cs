// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using AutoMapper;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Voting.Stimmunterlagen.MappingProfiles.Converter;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ConverterProfile : Profile
{
    public ConverterProfile()
    {
        CreateMap<Timestamp, DateTime>().ConvertUsing<ProtoTimestampConverter>();
        CreateMap<Timestamp, DateTime?>().ConvertUsing<ProtoTimestampConverter>();
        CreateMap<Timestamp, DateOnly>().ConvertUsing<ProtoTimestampConverter>();
        CreateMap<DateOnly, Timestamp>().ConvertUsing<ProtoTimestampConverter>();
        CreateMap<DateTime, Timestamp>().ConvertUsing<ProtoTimestampConverter>();
        CreateMap<DateTime?, Timestamp?>().ConvertUsing<ProtoTimestampConverter>();

        CreateMap<Guid?, string>().ConvertUsing<GuidStringConverter>();
        CreateMap<string, Guid?>().ConvertUsing<GuidStringConverter>();

        CreateMap(typeof(IEnumerable<>), typeof(RepeatedField<>))
            .ConvertUsing(typeof(RepeatedFieldConverter<,>));

        CreateMap(typeof(RepeatedField<>), typeof(RepeatedField<>))
            .ConvertUsing(typeof(RepeatedFieldConverter<,>));

        // this converter is invoked for all strings
        // (also not-nullable string types on the source side)
        // we use the correct nullable typing to match all type assertions
        CreateMap<string?, string>()
            .ConvertUsing<NullToEmptyStringConverter>();
    }
}
