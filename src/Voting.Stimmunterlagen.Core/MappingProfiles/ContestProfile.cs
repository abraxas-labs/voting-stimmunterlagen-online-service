// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class ContestProfile : Profile
{
    public ContestProfile()
    {
        CreateMap<ContestEventData, Contest>()
            .ForMember(dst => dst.Translations, opts => opts.MapFrom((src, _) => TranslationBuilder.CreateTranslations<ContestTranslation>(
                (t, x) => t.Description = x,
                src.Description)));
        CreateMap<PoliticalAssemblyEventData, Contest>()
            .ForMember(dst => dst.Translations, opts => opts.MapFrom((src, _) => TranslationBuilder.CreateTranslations<ContestTranslation>(
                (t, x) => t.Description = x,
                src.Description)));
    }
}
