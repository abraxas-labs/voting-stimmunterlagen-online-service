// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace Voting.Stimmunterlagen.MappingProfiles.Converter;

public class ProtoTimestampConverter :
    ITypeConverter<Timestamp, DateTime>,
    ITypeConverter<Timestamp, DateTime?>,
    ITypeConverter<Timestamp, DateOnly>,
    ITypeConverter<DateOnly, Timestamp>,
    ITypeConverter<DateTime, Timestamp>,
    ITypeConverter<DateTime?, Timestamp?>
{
    public DateTime Convert(Timestamp source, DateTime destination, ResolutionContext context)
        => source.ToDateTime();

    public DateOnly Convert(Timestamp source, DateOnly destination, ResolutionContext context)
        => DateOnly.FromDateTime(source.ToDateTime());

    public DateTime? Convert(Timestamp? source, DateTime? destination, ResolutionContext context)
        => source?.ToDateTime();

    public Timestamp Convert(DateOnly source, Timestamp destination, ResolutionContext context)
        => DateTime.SpecifyKind(source.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc).ToTimestamp();

    public Timestamp Convert(DateTime source, Timestamp destination, ResolutionContext context)
        => source.ToTimestamp();

    public Timestamp? Convert(DateTime? source, Timestamp? destination, ResolutionContext context)
        => source?.ToTimestamp();
}
