// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using AutoMapper;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class StistatProfile : Profile
{
    public StistatProfile()
    {
        CreateMap<PersonStistatCsvImportModel, PersonStistatCsvSimplifiedExportModel>();
    }
}
