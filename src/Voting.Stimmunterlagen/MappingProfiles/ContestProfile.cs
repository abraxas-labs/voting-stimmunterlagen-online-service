// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Lib.Database.Models;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Responses;
using Contest = Voting.Stimmunterlagen.Data.Models.Contest;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ContestProfile : Profile
{
    public ContestProfile()
    {
        CreateMap<Contest, ProtoModels.Contest>();
        CreateMap<ProtoModels.Contest, ContestSummary>()
            .ForMember(dst => dst.Contest, opts => opts.MapFrom(src => src))
            .ForMember(dst => dst.PrintJobState, opts => opts.MapFrom(src => src.LowestPrintJobsState))
            .ReverseMap();
        CreateMap<ContestCommunalDeadlinesCalculationResult, SetCommunalContestDeadlinesResponse>();
        CreateMap<ContestCommunalDeadlinesCalculationResult, GetPreviewCommunalContestDeadlinesResponse>();
        CreateMap<Page<ContestSummary>, ListContestsResponse>()
            .ForMember(dst => dst.PageInfo, opts => opts.MapFrom(src => src))
            .ForMember(dst => dst.Contests, opts => opts.MapFrom(src => src.Items));
    }
}
