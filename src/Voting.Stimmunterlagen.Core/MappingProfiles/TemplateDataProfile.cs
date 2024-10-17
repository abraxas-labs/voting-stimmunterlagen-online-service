// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using DomainOfInfluence = Voting.Stimmunterlagen.Core.Models.TemplateData.DomainOfInfluence;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class TemplateDataProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateDataProfile"/> class.
    /// For now we don't map null values to prevent empty string values which are required by dmDoc.
    /// </summary>
    public TemplateDataProfile()
    {
        CreateMap<Contest, Models.TemplateData.Contest>()
            .ForMember(
                dst => dst.OrderNumber,
                opts => opts.MapFrom(src => DatamatrixMapping.MapContestOrderNumber(src.OrderNumber)));
        CreateMap<ContestDomainOfInfluence, DomainOfInfluence>()
            .ForMember(dst => dst.VotingCardColor, opts => opts.Ignore()); // Mapped manually
        CreateMap<Voter, Models.TemplateData.Voter>()
            .ForMember(dst => dst.ShipmentNumber, opts => opts.MapFrom(src => DatamatrixMapping.MapVoterShipmentNumber(src.ContestIndex)))
            .ForMember(dst => dst.PersonId, opts => opts.MapFrom(src => DatamatrixMapping.MapPersonId(src.PersonId)))
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
    }
}
