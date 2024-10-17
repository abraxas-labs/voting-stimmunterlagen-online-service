// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class DomainOfInfluenceVotingCardPrintDataProfile : Profile
{
    public DomainOfInfluenceVotingCardPrintDataProfile()
    {
        CreateMap<DomainOfInfluenceVotingCardPrintData, ProtoModels.DomainOfInfluenceVotingCardPrintData>();
    }
}
