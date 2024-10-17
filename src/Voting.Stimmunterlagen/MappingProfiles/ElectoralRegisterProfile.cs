// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using AutoMapper;
using Voting.Stimmregister.Proto.V1.Services.Models;
using Voting.Stimmunterlagen.Core.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Voting.Stimmunterlagen.Proto.V1.Responses;
using ElectoralRegisterFilter = Voting.Stimmunterlagen.Core.Models.ElectoralRegisterFilter;
using ElectoralRegisterFilterMetadata = Voting.Stimmunterlagen.Core.Models.ElectoralRegisterFilterMetadata;
using ElectoralRegisterFilterVersion = Voting.Stimmunterlagen.Core.Models.ElectoralRegisterFilterVersion;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;
using VoterList = Voting.Stimmunterlagen.Data.Models.VoterList;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class ElectoralRegisterProfile : Profile
{
    public ElectoralRegisterProfile()
    {
        CreateMap<CreateVoterListImportWithNewElectoralRegisterFilterVersionRequest, VoterListImportWithNewElectoralRegisterFilter>();
        CreateMap<UpdateVoterListImportWithNewElectoralRegisterFilterVersionRequest, VoterListImportWithNewElectoralRegisterFilter>();
        CreateMap<FilterDefinitionModel, ElectoralRegisterFilter>();
        CreateMap<IEnumerable<FilterDefinitionModel>, List<ElectoralRegisterFilter>>();
        CreateMap<FilterVersionModel, ElectoralRegisterFilterVersion>()
            .ForCtorParam(nameof(ElectoralRegisterFilterVersion.NumberOfPersons), opt => opt.MapFrom(src => src.Count))
            .ForCtorParam(nameof(ElectoralRegisterFilterVersion.NumberOfInvalidPersons), opt => opt.MapFrom(src => src.CountOfInvalidPersons))
            .ForCtorParam(nameof(ElectoralRegisterFilterVersion.CreatedAt), opt => opt.MapFrom(src => src.AuditInfo.CreatedAt))
            .ForCtorParam(nameof(ElectoralRegisterFilterVersion.CreatedByName), opt => opt.MapFrom(src => src.AuditInfo.CreatedByName));
        CreateMap<IEnumerable<FilterVersionModel>, List<ElectoralRegisterFilterVersion>>();
        CreateMap<ElectoralRegisterFilter, ProtoModels.ElectoralRegisterFilter>();
        CreateMap<IEnumerable<ElectoralRegisterFilter>, ProtoModels.ElectoralRegisterFilters>()
            .ForMember(dst => dst.Filters, opt => opt.MapFrom(src => src));
        CreateMap<ElectoralRegisterFilterVersion, ProtoModels.ElectoralRegisterFilterVersion>();
        CreateMap<IEnumerable<ElectoralRegisterFilterVersion>, ProtoModels.ElectoralRegisterFilterVersions>()
            .ForMember(dst => dst.Versions, opt => opt.MapFrom(src => src));
        CreateMap<ElectoralRegisterFilterMetadata, ProtoModels.ElectoralRegisterFilterMetadata>();
        CreateMap<VoterList, VoterListImportVoterListResponse>();
    }
}
