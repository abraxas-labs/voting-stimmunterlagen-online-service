// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Ech0045_4_0;
using Ech0155_4_0;
using Voting.Stimmunterlagen.Data.Models;
using DomainOfInfluenceType = Voting.Stimmunterlagen.Data.Models.DomainOfInfluenceType;

namespace Voting.Stimmunterlagen.Ech.Mapping;

internal static class DomainOfInfluenceMapping
{
    public static List<VotingPersonTypeDomainOfInfluenceInfo> ToEchDomainOfInfluenceInfo(
        this ContestDomainOfInfluence doi,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        var countingCircles = doi.CountingCircles!
            .Select(x => x.CountingCircle!.ToEchCountingCircle())
            .ToList();
        var parentsAndSelfDomainOfInfluences = doiHierarchyByDoiId[doi.Id];

        var doiInfos = new List<VotingPersonTypeDomainOfInfluenceInfo>();
        foreach (var hierarchyDoi in parentsAndSelfDomainOfInfluences.OrderBy(x => x.Id))
        {
            foreach (var countingCircle in countingCircles.OrderBy(x => x.CountingCircleId))
            {
                doiInfos.Add(new VotingPersonTypeDomainOfInfluenceInfo
                {
                    DomainOfInfluence = hierarchyDoi.ToEchDomainOfInfluence(),
                    CountingCircle = countingCircle,
                });
            }
        }

        return doiInfos;
    }

    public static Ech0155_4_0.DomainOfInfluenceType ToEchDomainOfInfluence(this ContestDomainOfInfluence doi)
    {
        return new()
        {
            LocalDomainOfInfluenceIdentification = doi.BasisDomainOfInfluenceId.ToString(),
            DomainOfInfluenceTypeProperty = doi.Type.ToEchDomainOfInfluenceType(),
            DomainOfInfluenceName = doi.Name,
        };
    }

    public static DomainOfInfluenceType ToDomainOfInfluenceType(this DomainOfInfluenceTypeType type)
    {
        return type switch
        {
            DomainOfInfluenceTypeType.Ch => DomainOfInfluenceType.Ch,
            DomainOfInfluenceTypeType.Ct => DomainOfInfluenceType.Ct,
            DomainOfInfluenceTypeType.Bz => DomainOfInfluenceType.Bz,
            DomainOfInfluenceTypeType.Mu => DomainOfInfluenceType.Mu,
            DomainOfInfluenceTypeType.Sc => DomainOfInfluenceType.Sc,
            DomainOfInfluenceTypeType.Ki => DomainOfInfluenceType.Ki,
            DomainOfInfluenceTypeType.Og => DomainOfInfluenceType.Og,
            DomainOfInfluenceTypeType.Ko => DomainOfInfluenceType.Ko,
            DomainOfInfluenceTypeType.An => DomainOfInfluenceType.An,
            _ => throw new InvalidOperationException("invalid ech domain of influence type " + type),
        };
    }

    public static DomainOfInfluenceTypeType ToEchDomainOfInfluenceType(this DomainOfInfluenceType type)
    {
        return type switch
        {
            DomainOfInfluenceType.Ch => DomainOfInfluenceTypeType.Ch,
            DomainOfInfluenceType.Ct => DomainOfInfluenceTypeType.Ct,
            DomainOfInfluenceType.Bz => DomainOfInfluenceTypeType.Bz,
            DomainOfInfluenceType.Mu => DomainOfInfluenceTypeType.Mu,
            DomainOfInfluenceType.Sc => DomainOfInfluenceTypeType.Sc,
            DomainOfInfluenceType.Ki => DomainOfInfluenceTypeType.Ki,
            DomainOfInfluenceType.Og => DomainOfInfluenceTypeType.Og,
            DomainOfInfluenceType.Ko => DomainOfInfluenceTypeType.Ko,
            DomainOfInfluenceType.An => DomainOfInfluenceTypeType.An,
            _ => throw new InvalidOperationException("invalid domain of influence type " + type),
        };
    }
}
