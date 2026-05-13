// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class DomainOfInfluenceVotingCardLayoutProfile : Profile
{
    public DomainOfInfluenceVotingCardLayoutProfile()
    {
        CreateMap<DomainOfInfluenceVotingCardLayout, ProtoModels.DomainOfInfluenceVotingCardLayout>()
            .ForMember(dst => dst.ContestTemplate, opts => opts.MapFrom(x => x.Template));
        CreateMap<IEnumerable<DomainOfInfluenceVotingCardLayout>, ProtoModels.DomainOfInfluenceVotingCardLayouts>()
            .ForMember(dst => dst.Layouts, opts => opts.MapFrom(x => x));

        CreateMap<IEnumerable<GroupedDomainOfInfluenceVotingCardLayouts>, ProtoModels.GroupedDomainOfInfluenceVotingCardLayouts>()
            .ForMember(dst => dst.LayoutGroups, opts => opts.MapFrom(src => src));
        CreateMap<GroupedDomainOfInfluenceVotingCardLayouts, ProtoModels.GroupedDomainOfInfluenceVotingCardLayout>();

        CreateMap<IEnumerable<GroupedTemplateValues>, ProtoModels.TemplateDataValues>()
            .ForMember(dst => dst.Containers, opts => opts.MapFrom(src => src));
        CreateMap<TemplateDataContainer, ProtoModels.TemplateDataValuesContainer>();
        CreateMap<GroupedTemplateValues, ProtoModels.TemplateDataValuesContainer>()
            .IncludeMembers(src => src.Container)
            .ForMember(dst => dst.Fields, opts => opts.MapFrom(src => src.FieldValues));
        CreateMap<TemplateDataFieldValue, ProtoModels.TemplateDataFieldValue>()
            .IncludeMembers(src => src.Field);
        CreateMap<TemplateDataField, ProtoModels.TemplateDataFieldValue>();

        CreateMap<SetTemplateDataFieldRequest, SimpleTemplateFieldValue>();

        CreateMap<TemplateBrick, ProtoModels.TemplateBrick>();
        CreateMap<IEnumerable<TemplateBrick>, ProtoModels.TemplateBricks>()
            .ForMember(dst => dst.Bricks, opts => opts.MapFrom(src => src));
    }
}
