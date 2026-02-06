// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class EmptyVoterBuilder
{
    public static List<Voter> BuildEmptyVoters(string bfs, int count, int? pagesPerVoter = null)
    {
        var voters = new List<Voter>();
        var hasPagesPerVoter = pagesPerVoter.HasValue && pagesPerVoter.Value > 0;

        for (var i = 0; i < count; i++)
        {
            var currentPage = (i * pagesPerVoter.GetValueOrDefault()) + 1;
            voters.Add(BuildEmptyVoter(
                bfs,
                !hasPagesPerVoter ? 0 : currentPage,
                !hasPagesPerVoter ? 0 : currentPage + pagesPerVoter!.Value - 1));
        }

        return voters;
    }

    public static Voter BuildEmptyVoter(string bfs, int? pageFrom = null, int? pageTo = null)
    {
        return new()
        {
            VotingCardType = VotingCardType.Swiss,
            ListId = Guid.Empty,
            SendVotingCardsToDomainOfInfluenceReturnAddress = true,
            PageInfo = pageFrom != null && pageTo != null
                ? new()
                {
                    PageFrom = pageFrom.Value,
                    PageTo = pageTo.Value,
                }
                : null,
            Bfs = bfs,
        };
    }
}
