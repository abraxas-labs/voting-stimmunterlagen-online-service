// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class CountingCircleProfile : Profile
{
    public CountingCircleProfile()
    {
        CreateMap<CountingCircleEventData, CountingCircle>()
            .ForMember(dst => dst.SecureConnectId, opts => opts.MapFrom(src => src.ResponsibleAuthority.SecureConnectId));
        CreateMap<CountingCircleEventData, ContestCountingCircle>()
            .ForMember(dst => dst.SecureConnectId, opts => opts.MapFrom(src => src.ResponsibleAuthority.SecureConnectId));
        CreateMap<CountingCircle, ContestCountingCircle>();
    }
}
