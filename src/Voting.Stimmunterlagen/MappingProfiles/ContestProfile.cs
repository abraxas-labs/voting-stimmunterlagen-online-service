// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Responses;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ContestProfile : Profile
{
    public ContestProfile()
    {
        CreateMap<IEnumerable<Contest>, ProtoModels.Contests>()
            .ForMember(dst => dst.Contests_, opts => opts.MapFrom(src => src));
        CreateMap<IEnumerable<ContestSummary>, ProtoModels.Contests>()
            .ForMember(dst => dst.Contests_, opts => opts.MapFrom(src => src));
        CreateMap<Contest, ProtoModels.Contest>();
        CreateMap<ProtoModels.Contest, ContestSummary>()
            .ForMember(dst => dst.Contest, opts => opts.MapFrom(src => src))
            .ForMember(dst => dst.PrintJobState, opts => opts.MapFrom(src => src.LowestPrintJobsState))
            .ReverseMap();
        CreateMap<ContestCommunalDeadlinesCalculationResult, SetCommunalContestDeadlinesResponse>();
        CreateMap<ContestCommunalDeadlinesCalculationResult, GetPreviewCommunalContestDeadlinesResponse>();
    }
}
