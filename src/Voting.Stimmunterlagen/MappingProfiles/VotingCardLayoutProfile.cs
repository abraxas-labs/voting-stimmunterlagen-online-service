// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VotingCardLayoutProfile : Profile
{
    public VotingCardLayoutProfile()
    {
        CreateMap<VotingCardLayoutDataConfiguration, ProtoModels.VotingCardLayoutDataConfiguration>();
        CreateMap<EnterVotingCardLayoutDataConfigurationRequest, VotingCardLayoutDataConfiguration>();
    }
}
