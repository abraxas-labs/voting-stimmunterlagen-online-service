// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class StepProfile : Profile
{
    public StepProfile()
    {
        CreateMap<StepState, ProtoModels.StepState>();
        CreateMap<IEnumerable<StepState>, ProtoModels.StepStates>()
            .ForMember(dst => dst.Steps, opts => opts.MapFrom(x => x));
    }
}
