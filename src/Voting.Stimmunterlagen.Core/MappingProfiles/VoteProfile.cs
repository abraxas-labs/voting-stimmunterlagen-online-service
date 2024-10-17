// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class VoteProfile : Profile
{
    public VoteProfile()
    {
        CreateMap<VoteEventData, PoliticalBusiness>()
            .ForMember(dst => dst.Translations, opts => opts.MapFrom((src, _) => TranslationBuilder.CreateTranslations<PoliticalBusinessTranslation>(
                (t, x) => t.ShortDescription = x,
                src.ShortDescription,
                (t, x) => t.OfficialDescription = x,
                src.OfficialDescription)))
            .AfterMap((src, dest) => dest.PoliticalBusinessType = PoliticalBusinessType.Vote);
    }
}
