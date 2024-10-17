// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.EVoting.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class EVotingExportProfile : Profile
{
    public EVotingExportProfile()
    {
        CreateMap<Data.Models.ContestDomainOfInfluence, DomainOfInfluence>();
        CreateMap<Data.Models.Contest, Contest>();
        CreateMap<Data.Models.DomainOfInfluenceVotingCardPrintData, DomainOfInfluenceVotingCardPrintData>();
        CreateMap<Data.Models.DomainOfInfluenceVotingCardReturnAddress, DomainOfInfluenceVotingCardReturnAddress>();
    }
}
