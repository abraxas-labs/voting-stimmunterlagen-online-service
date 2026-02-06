// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class VotingCardLayoutProfile : Profile
{
    public VotingCardLayoutProfile()
    {
        CreateMap<VotingCardLayoutDataConfiguration, VotingCardLayoutDataConfiguration>();
    }
}
