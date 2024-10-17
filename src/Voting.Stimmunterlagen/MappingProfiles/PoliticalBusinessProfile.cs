// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class PoliticalBusinessProfile : Profile
{
    public PoliticalBusinessProfile()
    {
        CreateMap<PoliticalBusiness, ProtoModels.PoliticalBusiness>();
        CreateMap<IEnumerable<PoliticalBusiness>, ProtoModels.PoliticalBusinesses>()
            .ForMember(dst => dst.PoliticalBusinesses_, opts => opts.MapFrom(x => x));
    }
}
