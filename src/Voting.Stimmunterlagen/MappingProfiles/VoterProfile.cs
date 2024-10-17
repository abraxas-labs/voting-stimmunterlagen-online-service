// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Globalization;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Mapping;
using Voting.Stimmunterlagen.Models.Response;
using ProtoModels = Voting.Stimmunterlagen.Proto.V1.Models;

namespace Voting.Stimmunterlagen.MappingProfiles;

public class VoterProfile : Profile
{
    public VoterProfile()
    {
        CreateMap<Voter, ProtoModels.ManualVotingCardVoter>()
            .ForMember(dst => dst.ForeignZipCode, opts => opts.Condition(x => x.ForeignZipCode != null))
            .ForMember(dst => dst.SwissZipCode, opts => opts.Condition(x => x.SwissZipCode != null))
            .ForMember(dst => dst.DateOfBirth, opts => opts.MapFrom(x =>
                !string.IsNullOrEmpty(x.DateOfBirth) && x.DateOfBirth != DatePartiallyKnownMapping.UnspecifiedDateString
                    ? DateTime.SpecifyKind(DateTime.ParseExact(x.DateOfBirth, DatePartiallyKnownMapping.YearMonthDayFormat, CultureInfo.InvariantCulture), DateTimeKind.Utc).ToTimestamp()
                    : null))
            .ReverseMap()
            .ForMember(dst => dst.ForeignZipCode, opts => opts.Condition(x => x.ZipCodeCase == ProtoModels.ManualVotingCardVoter.ZipCodeOneofCase.ForeignZipCode))
            .ForMember(dst => dst.SwissZipCode, opts => opts.Condition(x => x.ZipCodeCase == ProtoModels.ManualVotingCardVoter.ZipCodeOneofCase.SwissZipCode))
            .ForMember(dst => dst.DateOfBirth, opts => opts.MapFrom(src => src.DateOfBirth.ToDateTime().ToString(DatePartiallyKnownMapping.YearMonthDayFormat)));
        CreateMap<Country, ProtoModels.Country>()
            .ReverseMap();
        CreateMap<VoterDuplicate, ProtoModels.VoterDuplicate>();
        CreateMap<VoterDuplicate, CreateVoterDuplicateResponse>();
    }
}
