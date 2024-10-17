// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
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
            .ForMember(dst => dst.DateOfBirth, opts => opts.MapFrom(x => x.DateOfBirth.ToDateTime().ToString("yyyy-MM-dd")));
        CreateMap<CreateCountryRequest, Country>();
        CreateMap<ManualVotingCardGeneratorJob, ProtoModels.ManualVotingCardGeneratorJob>();
    }
}
