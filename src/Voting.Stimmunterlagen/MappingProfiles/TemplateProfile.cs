// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class TemplateProfile : Profile
{
    public TemplateProfile()
    {
        CreateMap<Template, ProtoModels.Template>();
        CreateMap<IEnumerable<Template>, ProtoModels.Templates>()
            .ForMember(dst => dst.Templates_, opts => opts.MapFrom(src => src));
    }
}
