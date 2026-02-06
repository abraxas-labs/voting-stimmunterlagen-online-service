// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Ech0045_4_0;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping.V4;

internal static class VoterListMapping
{
    internal static VoterListType ToEchVoterList(
        this VoterList voterList,
        Contest contest,
        DomainOfInfluenceCanton canton,
        Dictionary<Guid, List<ContestDomainOfInfluence>> doiHierarchyByDoiId)
    {
        var voters = voterList.Voters?
            .OrderBy(v => v.Bfs)
            .ThenBy(v => v.Town)
            .ThenBy(v => v.LastName)
            .ThenBy(v => v.FirstName)
            .Select(v => v.ToEchVoter(voterList.VotingCardType, doiHierarchyByDoiId))
            .ToList()
            ?? new List<VotingPersonType>();

        var stringifiedCanton = canton.ToString().ToUpperInvariant();
        return new VoterListType
        {
            ReportingAuthority = new AuthorityType
            {
                OtherRegister = new OtherRegisterType
                {
                    RegisterIdentification = stringifiedCanton,
                    RegisterName = stringifiedCanton,
                },
            },
            Contest = contest.ToEchContestType(),
            NumberOfVoters = voterList.NumberOfVoters.ToString(),
            Voter = voters,
        };
    }
}
