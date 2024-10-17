// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class PrintJobProfile : Profile
{
    public PrintJobProfile()
    {
        CreateMap<IEnumerable<PrintJob>, ProtoModels.PrintJobs>()
            .ForMember(dst => dst.PrintJobs_, opts => opts.MapFrom(src => src));
        CreateMap<PrintJob, ProtoModels.PrintJob>()
            .ForMember(dst => dst.GenerateVotingCardsTriggered, opts => opts.MapFrom(src => src.DomainOfInfluence!.GenerateVotingCardsTriggered));

        CreateMap<IEnumerable<PrintJobSummary>, ProtoModels.PrintJobSummaries>()
            .ForMember(dst => dst.Summaries, opts => opts.MapFrom(src => src));
        CreateMap<PrintJobSummary, ProtoModels.PrintJobSummary>()
            .ForMember(dst => dst.GenerateVotingCardsTriggered, opts => opts.MapFrom(src => src.DomainOfInfluence!.GenerateVotingCardsTriggered));
    }
}
