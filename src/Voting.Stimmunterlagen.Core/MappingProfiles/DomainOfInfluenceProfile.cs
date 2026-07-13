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
        CreateMap<DomainOfInfluenceEventData, DomainOfInfluence>()
            .ForMember(
                dst => dst.StistatExportEaiMessageType,
                opts => opts.Condition(src => src.StistatExportEaiMessageTypeSupported));
        CreateMap<DomainOfInfluenceEventData, ContestDomainOfInfluence>()
            .ForMember(
                dst => dst.StistatExportEaiMessageType,
                opts => opts.Condition(src => src.StistatExportEaiMessageTypeSupported));
        CreateMap<DomainOfInfluence, ContestDomainOfInfluence>()
            .ForMember(dst => dst.LogoRef, opts => opts.PreCondition(x => x.LogoRef != null));
        CreateMap<DomainOfInfluenceCantonDefaults, DomainOfInfluenceCantonDefaults>();
        CreateMap<DomainOfInfluenceHierarchyEntry, ContestDomainOfInfluenceHierarchyEntry>()
            .ForMember(x => x.DomainOfInfluence, opts => opts.Ignore())
            .ForMember(x => x.ParentDomainOfInfluence, opts => opts.Ignore());
        CreateMap<DomainOfInfluenceVotingCardPrintDataEventData, DomainOfInfluenceVotingCardPrintData>();
        CreateMap<DomainOfInfluenceVotingCardSwissPostDataEventData, DomainOfInfluenceVotingCardSwissPostData>();
        CreateMap<DomainOfInfluenceVotingCardReturnAddressEventData, DomainOfInfluenceVotingCardReturnAddress>();
        CreateMap<DomainOfInfluenceVotingCardDataUpdated, DomainOfInfluence>()
            .ForMember(
                dst => dst.StistatExportEaiMessageType,
                opts => opts.Condition(src => !src.StistatExportEaiMessageTypeDeprecated));
        CreateMap<DomainOfInfluenceVotingCardDataUpdated, ContestDomainOfInfluence>()
            .ForMember(
                dst => dst.StistatExportEaiMessageType,
                opts => opts.Condition(src => !src.StistatExportEaiMessageTypeDeprecated));
    }
}
