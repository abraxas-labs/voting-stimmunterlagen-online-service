// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0010_6_0;
using Ech0044_4_1;
using DataModels = Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping;

internal static class SexMapping
{
    public static SexType ToEchSexType(this DataModels.SexType sex)
    {
        return sex switch
        {
            DataModels.SexType.Male => SexType.Item1,
            DataModels.SexType.Female => SexType.Item2,
            _ => SexType.Item3,
        };
    }

    public static DataModels.SexType ToSexType(this SexType sex)
    {
        return sex switch
        {
            SexType.Item1 => DataModels.SexType.Male,
            SexType.Item2 => DataModels.SexType.Female,
            _ => DataModels.SexType.Undefined,
        };
    }

    public static MrMrsType ToEchMrMrsType(this DataModels.Salutation salutation)
    {
        return salutation switch
        {
            DataModels.Salutation.Mistress => MrMrsType.Item1,
            DataModels.Salutation.Mister => MrMrsType.Item2,
            DataModels.Salutation.Miss => MrMrsType.Item3,
            _ => throw new ArgumentOutOfRangeException(nameof(salutation), salutation, "invalid salutation"),
        };
    }

    public static DataModels.Salutation ToSalutation(this MrMrsType? mrMrsType)
    {
        return mrMrsType switch
        {
            MrMrsType.Item1 => DataModels.Salutation.Mistress,
            MrMrsType.Item2 => DataModels.Salutation.Mister,
            MrMrsType.Item3 => DataModels.Salutation.Miss,
            _ => DataModels.Salutation.Unspecified,
        };
    }
}
