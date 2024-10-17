// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class DomainOfInfluenceProfile : Profile
{
    public DomainOfInfluenceProfile()
    {
        CreateMap<DomainOfInfluenceEventData, DomainOfInfluence>();
        CreateMap<DomainOfInfluenceEventData, ContestDomainOfInfluence>();
        CreateMap<DomainOfInfluence, ContestDomainOfInfluence>()
            .ForMember(dst => dst.LogoRef, opts => opts.PreCondition(x => x.LogoRef != null));
        CreateMap<DomainOfInfluenceCantonDefaults, DomainOfInfluenceCantonDefaults>();
        CreateMap<DomainOfInfluenceHierarchyEntry, ContestDomainOfInfluenceHierarchyEntry>()
            .ForMember(x => x.DomainOfInfluence, opts => opts.Ignore())
            .ForMember(x => x.ParentDomainOfInfluence, opts => opts.Ignore());
        CreateMap<DomainOfInfluenceVotingCardPrintDataEventData, DomainOfInfluenceVotingCardPrintData>();
        CreateMap<DomainOfInfluenceVotingCardSwissPostDataEventData, DomainOfInfluenceVotingCardSwissPostData>();
        CreateMap<DomainOfInfluenceVotingCardReturnAddressEventData, DomainOfInfluenceVotingCardReturnAddress>();
        CreateMap<DomainOfInfluenceVotingCardDataUpdated, DomainOfInfluence>();
        CreateMap<DomainOfInfluenceVotingCardDataUpdated, ContestDomainOfInfluence>();
    }
}
