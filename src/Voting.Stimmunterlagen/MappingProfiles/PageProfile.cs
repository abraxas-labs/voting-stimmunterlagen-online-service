// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Lib.Database.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class PageProfile : Profile
{
    public PageProfile()
    {
        // write
        CreateMap<ProtoModels.Pageable, Pageable>()
            .ConstructUsing(src => new Pageable(src.Page, src.PageSize));

        // read
        CreateMap(typeof(Page<>), typeof(ProtoModels.PageInfo));
    }
}
