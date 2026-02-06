// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Models.Request;
using Voting.Stimmunterlagen.Models.Response;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;
using VoterListImport = Voting.Stimmunterlagen.Data.Models.VoterListImport;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VoterListImportProfile : Profile
{
    public VoterListImportProfile()
    {
        CreateMap<UpdateVoterListImportRequest, VoterListImport>();
        CreateMap<CreateVoterListImportRequest, VoterListImport>();
        CreateMap<VoterListImport, ProtoModels.VoterListImport>();
        CreateMap<VoterDuplicateKey, ProtoModels.VoterDuplicate>();
        CreateMap<VoterDuplicateKey, VoterDuplicateResponse>();
        CreateMap<VoterListImportResult, Proto.V1.Responses.VoterListImportError>();
        CreateMap<VoterListImportResult, VoterListImportErrorResponse>();
    }
}
