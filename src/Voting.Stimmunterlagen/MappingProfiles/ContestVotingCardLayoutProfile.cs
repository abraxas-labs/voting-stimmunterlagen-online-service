// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ContestVotingCardLayoutProfile : Profile
{
    public ContestVotingCardLayoutProfile()
    {
        CreateMap<Data.Models.ContestVotingCardLayout, ProtoModels.ContestVotingCardLayout>();
        CreateMap<IEnumerable<Data.Models.ContestVotingCardLayout>, ProtoModels.ContestVotingCardLayouts>()
            .ForMember(dst => dst.Layouts, opts => opts.MapFrom(x => x));
    }
}
