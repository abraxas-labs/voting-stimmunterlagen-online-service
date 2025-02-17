// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using AutoMapper;
using Google.Protobuf.Collections;

namespace Voting.Stimmunterlagen.MappingProfiles.Converter;

public class RepeatedFieldConverter<TItemSource, TItemDest> :
    ITypeConverter<IEnumerable<TItemSource>, RepeatedField<TItemDest>>
{
    public RepeatedField<TItemDest> Convert(
        IEnumerable<TItemSource> source,
        RepeatedField<TItemDest> destination,
        ResolutionContext context)
    {
        if (destination == null)
        {
            throw new InvalidOperationException("Invalid mapping configuration: Repeated fields are always readonly and therefore cannot be null. Adding a mapping rule for this member usually fixes this problem.");
        }

        foreach (var item in source)
        {
            destination.Add(context.Mapper.Map<TItemDest>(item));
        }

        return destination;
    }
}
