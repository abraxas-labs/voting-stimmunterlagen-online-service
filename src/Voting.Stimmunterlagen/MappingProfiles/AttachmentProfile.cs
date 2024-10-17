// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class AttachmentProfile : Profile
{
    public AttachmentProfile()
    {
        CreateMap<CreateAttachmentRequest, Attachment>()
            .ForMember(dst => dst.PoliticalBusinessEntries, opts => opts.MapFrom(src => src.PoliticalBusinessIds.Select(pbId => new PoliticalBusinessAttachmentEntry { PoliticalBusinessId = GuidParser.Parse(pbId) })));
        CreateMap<UpdateAttachmentRequest, Attachment>()
            .ForMember(dst => dst.PoliticalBusinessEntries, opts => opts.MapFrom(src => src.PoliticalBusinessIds.Select(pbId => new PoliticalBusinessAttachmentEntry { AttachmentId = GuidParser.Parse(src.Id), PoliticalBusinessId = GuidParser.Parse(pbId) })));
        CreateMap<Attachment, ProtoModels.Attachment>()
            .ForMember(dst => dst.PoliticalBusinessIds, opts => opts.MapFrom(src => src.PoliticalBusinessEntries!.Select(x => x.PoliticalBusinessId)))
            .ForMember(dst => dst.DomainOfInfluenceAttachmentRequiredCount, opts => opts.MapFrom(src => src.DomainOfInfluenceAttachmentCounts!.Count != 1 ? null : src.DomainOfInfluenceAttachmentCounts!.First().RequiredCount))
            .ForMember(dst => dst.DomainOfInfluenceAttachmentRequiredForVoterListsCount, opts => opts.MapFrom(src => src.DomainOfInfluenceAttachmentCounts!.Count != 1 ? (int?)null : src.DomainOfInfluenceAttachmentCounts!.First().RequiredForVoterListsCount));
        CreateMap<IEnumerable<Attachment>, ProtoModels.Attachments>()
            .ForMember(dst => dst.Attachments_, opts => opts.MapFrom(src => src));

        CreateMap<AttachmentCategorySummary, ProtoModels.AttachmentCategorySummary>();
        CreateMap<IEnumerable<AttachmentCategorySummary>, ProtoModels.AttachmentCategorySummaries>()
            .ForMember(dst => dst.Summaries, opts => opts.MapFrom(src => src));

        CreateMap<DomainOfInfluenceAttachmentCategorySummariesEntry, ProtoModels.DomainOfInfluenceAttachmentCategorySummariesEntry>();
        CreateMap<IEnumerable<DomainOfInfluenceAttachmentCategorySummariesEntry>, ProtoModels.DomainOfInfluenceAttachmentCategorySummariesEntries>()
            .ForMember(dst => dst.Entries, opts => opts.MapFrom(src => src));

        CreateMap<DomainOfInfluenceAttachmentCount, ProtoModels.DomainOfInfluenceAttachmentCount>();
        CreateMap<IEnumerable<DomainOfInfluenceAttachmentCount>, ProtoModels.DomainOfInfluenceAttachmentCounts>()
            .ForMember(dst => dst.Counts, opts => opts.MapFrom(src => src));
    }
}
