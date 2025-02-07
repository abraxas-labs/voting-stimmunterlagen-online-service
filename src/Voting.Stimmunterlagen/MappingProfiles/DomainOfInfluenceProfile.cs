// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class DomainOfInfluenceProfile : Profile
{
    public DomainOfInfluenceProfile()
    {
        CreateMap<ContestDomainOfInfluence, ProtoModels.DomainOfInfluence>()
            .ForMember(dst => dst.EVoting, opts => opts.MapFrom(src => src.CountingCircles != null ? src.CountingCircles.Any(cc => cc.CountingCircle!.EVoting) : (bool?)null))
            .IncludeMembers(x => x.CantonDefaults);
        CreateMap<DomainOfInfluenceCantonDefaults, ProtoModels.DomainOfInfluence>();
        CreateMap<IEnumerable<ContestDomainOfInfluence>, ProtoModels.DomainOfInfluences>()
            .ForMember(dst => dst.DomainOfInfluences_, opts => opts.MapFrom(x => x));

        CreateMap<EVotingDomainOfInfluenceEntry, ProtoModels.EVotingDomainOfInfluenceEntry>();
        CreateMap<IEnumerable<EVotingDomainOfInfluenceEntry>, ProtoModels.EVotingDomainOfInfluenceEntries>()
            .ForMember(dst => dst.Entries, opts => opts.MapFrom(x => x));
    }
}
