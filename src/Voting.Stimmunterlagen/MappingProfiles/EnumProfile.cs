// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using Voting.Stimmunterlagen.Data.Models;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

/// <summary>
/// An entry for every enum is needed since AutoMapper doesnt have a property to enable mapByName globally.
/// If an entry for an enum is not defined AutoMapper will map the enums by their values.
/// </summary>
public class EnumProfile : Profile
{
    public EnumProfile()
    {
        CreateEnumMap<Step, ProtoModels.Step>();
        CreateEnumMap<VotingCardType, ProtoModels.VotingCardType>();
        CreateEnumMap<AttachmentFormat, ProtoModels.AttachmentFormat>();
        CreateEnumMap<AttachmentState, ProtoModels.AttachmentState>();
        CreateEnumMap<ContestState, ProtoModels.ContestState>();
        CreateEnumMap<DomainOfInfluenceType, ProtoModels.DomainOfInfluenceType>();
        CreateEnumMap<PoliticalBusinessType, ProtoModels.PoliticalBusinessType>();
        CreateEnumMap<VotingCardGroup, ProtoModels.VotingCardGroup>();
        CreateEnumMap<VotingCardSort, ProtoModels.VotingCardSort>();
        CreateEnumMap<PrintJobState, ProtoModels.PrintJobState>();
        CreateEnumMap<VotingCardGeneratorJobState, ProtoModels.VotingCardGeneratorJobState>();

        // explicitly map deprecated values to default value.
        CreateMap<Abraxas.Voting.Basis.Shared.V1.VotingCardColor, VotingCardColor>()
            .ConvertUsingEnumMapping(opt => opt
                .MapByValue()
                .MapValue(Abraxas.Voting.Basis.Shared.V1.VotingCardColor.Chamois, VotingCardColor.Unspecified)
                .MapValue(Abraxas.Voting.Basis.Shared.V1.VotingCardColor.Gold, VotingCardColor.Unspecified))
            .ReverseMap();
    }

    private void CreateEnumMap<T1, T2>()
        where T1 : struct, Enum
        where T2 : struct, Enum
    {
        CreateMap<T1, T2>()
            .ConvertUsingEnumMapping(opt => opt.MapByName())
            .ReverseMap();
    }
}
