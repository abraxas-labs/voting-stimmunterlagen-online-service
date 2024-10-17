// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VotingCardPrintFileExportJobProfile : Profile
{
    public VotingCardPrintFileExportJobProfile()
    {
        CreateMap<IEnumerable<VotingCardPrintFileExportJob>, ProtoModels.VotingCardPrintFileExportJobs>()
            .ForMember(dst => dst.Jobs, opts => opts.MapFrom(x => x));
        CreateMap<VotingCardPrintFileExportJob, ProtoModels.VotingCardPrintFileExportJob>();
    }
}
