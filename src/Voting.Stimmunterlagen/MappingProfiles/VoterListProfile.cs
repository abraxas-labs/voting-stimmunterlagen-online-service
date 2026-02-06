// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Models.Response;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;
using VoterList = Voting.Stimmunterlagen.Data.Models.VoterList;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VoterListProfile : Profile
{
    public VoterListProfile()
    {
        CreateMap<Guid, PoliticalBusinessVoterListEntry>()
            .ConstructUsing(x => new PoliticalBusinessVoterListEntry { PoliticalBusinessId = x });
        CreateMap<VoterListManager.VoterListsData, ProtoModels.VoterLists>()
            .ForMember(dst => dst.VoterLists_, opts => opts.MapFrom(src => src.VoterLists));
        CreateMap<VoterListManager.PoliticalBusinessCountOfVotingCards, ProtoModels.PoliticalBusinessCountOfVotingCards>();
        CreateMap<VoterList, ProtoModels.VoterList>()
            .ForMember(dst => dst.PoliticalBusinessIds, opts => opts.MapFrom(x => x.PoliticalBusinessEntries!.Select(y => y.PoliticalBusinessId)))
            .ForMember(dst => dst.Name, opts => opts.MapFrom(src => src.Import!.Name))
            .ForMember(dst => dst.LastUpdate, opts => opts.MapFrom(src => src.Import!.LastUpdate))
            .ForMember(dst => dst.Source, opts => opts.MapFrom(src => src.Import!.Source));
        CreateMap<VoterList, CreateUpdateVoterListResponse>();
    }
}
