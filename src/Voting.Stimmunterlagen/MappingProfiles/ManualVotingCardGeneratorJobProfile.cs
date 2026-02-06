// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.MappingProfiles.Converter;
using Voting.Stimmunterlagen.MappingProfiles.Resolver;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ManualVotingCardGeneratorJobProfile : Profile
{
    public ManualVotingCardGeneratorJobProfile()
    {
        CreateMap<IEnumerable<ManualVotingCardGeneratorJob>, ProtoModels.ManualVotingCardGeneratorJobs>()
            .ForMember(dst => dst.Jobs, opts => opts.MapFrom(x => x));
        CreateMap<CreateManualVotingCardVoterRequest, Voter>()
            .ForMember(dst => dst.DateOfBirth, opts => opts.MapFrom(src => src.DateOfBirth.ToDateTime().ToString("yyyy-MM-dd")))
            .ForMember(dst => dst.Religion, opts => opts.ConvertUsing(new ReligionConverter(), src => src.Religion))
            .ForMember(dst => dst.IsMinor, opts => opts.MapFrom(src => src.Religion == ProtoModels.Religion.CatholicMinorOrForeigner || src.Religion == ProtoModels.Religion.ProtestantMinorOrForeigner))
            .ForMember(dst => dst.DomainOfInfluences, opts => opts.MapFrom<DomainOfInfluencesResolver>());
        CreateMap<CreateCountryRequest, Country>();
        CreateMap<ManualVotingCardGeneratorJob, ProtoModels.ManualVotingCardGeneratorJob>();
    }
}
