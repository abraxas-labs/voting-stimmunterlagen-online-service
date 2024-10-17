// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class DomainOfInfluenceVotingCardConfigurationProfile : Profile
{
    public DomainOfInfluenceVotingCardConfigurationProfile()
    {
        CreateMap<Data.Models.DomainOfInfluenceVotingCardConfiguration, ProtoModels.DomainOfInfluenceVotingCardConfiguration>()
            .ForMember(dst => dst.VotingCardGroups, opts => opts.MapFrom(src => src.Groups))
            .ForMember(dst => dst.VotingCardSorts, opts => opts.MapFrom(src => src.Sorts));
    }
}
