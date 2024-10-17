// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VotingCardGeneratorJobProfile : Profile
{
    public VotingCardGeneratorJobProfile()
    {
        CreateMap<IEnumerable<VotingCardGeneratorJob>, ProtoModels.VotingCardGeneratorJobs>()
            .ForMember(dst => dst.Jobs, opts => opts.MapFrom(x => x));
        CreateMap<VotingCardGeneratorJob, ProtoModels.VotingCardGeneratorJob>()
            .ForMember(dst => dst.VotingCardType, opts => opts.MapFrom(x => x.Layout != null ? x.Layout.VotingCardType : VotingCardType.EVoting));
    }
}
