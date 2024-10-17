// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Models.Request;
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
    }
}
