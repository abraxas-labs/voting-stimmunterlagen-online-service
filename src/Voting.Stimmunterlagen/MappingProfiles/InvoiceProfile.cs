// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voting.Stimmunterlagen.Proto.V1.Responses;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        CreateMap<CreateAdditionalInvoicePositionRequest, AdditionalInvoicePosition>();
        CreateMap<UpdateAdditionalInvoicePositionRequest, AdditionalInvoicePosition>();
        CreateMap<AdditionalInvoicePosition, ProtoModels.AdditionalInvoicePosition>();
        CreateMap<IEnumerable<AdditionalInvoicePositionAvailableMaterial>, ListAdditionalInvoicePositionAvailableMaterialResponse>()
            .ForMember(dst => dst.Materials, opts => opts.MapFrom(src => src));
        CreateMap<AdditionalInvoicePositionAvailableMaterial, ProtoModels.AdditionalInvoicePositionAvailableMaterial>();

        CreateMap<IEnumerable<AdditionalInvoicePosition>, ProtoModels.AdditionalInvoicePositions>()
             .ForMember(dst => dst.Positions, opts => opts.MapFrom(src => src));
    }
}
