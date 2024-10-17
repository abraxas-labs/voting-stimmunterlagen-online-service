﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Events.V1.Data;
using AutoMapper;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.MappingProfiles;

public class CantonSettingsProfile : Profile
{
    public CantonSettingsProfile()
    {
        CreateMap<CantonSettingsEventData, CantonSettings>();
    }
}
