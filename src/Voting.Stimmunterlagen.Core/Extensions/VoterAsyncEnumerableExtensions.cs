// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Extensions;

public static partial class VoterAsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<Voter> OrderBySortingCriteriaAsync(
        this IAsyncEnumerable<Voter> voters, VotingCardSort[] sorts)
    {
        var voterList = await voters.ToListAsync();

        IOrderedEnumerable<Voter> sorted;

        if (sorts == null || sorts.Length == 0)
        {
            sorted = voterList.OrderBy(x => x.SourceIndex);
        }
        else
        {
            // Apply first sort
            sorted = OrderBy(voterList, sorts[0]);

            // Apply additional sorts
            for (int i = 1; i < sorts.Length; i++)
            {
                sorted = ThenBy(sorted, sorts[i]);
            }

            sorted = sorted.ThenBy(x => x.SourceIndex).ThenBy(x => x.PersonId);
        }

        foreach (var voter in sorted)
        {
            yield return voter; // Stream results asynchronously
        }
    }

    private static IOrderedEnumerable<Voter> OrderBy(IEnumerable<Voter> voters, VotingCardSort sort)
    {
        return sort switch
        {
            VotingCardSort.Street => voters.OrderBy(x => x.Street)
                                           .ThenBy(x => HouseNumberHelper.ExtractHouseNumber(x.HouseNumber)),

            VotingCardSort.Name => voters.OrderBy(x => x.LastName)
                                         .ThenBy(x => x.FirstName),

            VotingCardSort.Place => voters.OrderBy(x => x.Town),

            VotingCardSort.Denomination => voters.OrderBy(x => x.IsMinor)
            .ThenBy(x => VoterTypeHelper.GetVoterTypeSortValue(x.VoterType))
            .ThenBy(x => DenominationHelper.GetDenominationSortValue(x.Religion)),

            VotingCardSort.Household => voters.OrderByDescending(x => x.IsHouseholder),

            _ => voters.OrderBy(x => x.SourceIndex),
        };
    }

    private static IOrderedEnumerable<Voter> ThenBy(IOrderedEnumerable<Voter> voters, VotingCardSort sort)
    {
        return sort switch
        {
            VotingCardSort.Street => voters.ThenBy(x => x.Street)
                                           .ThenBy(x => HouseNumberHelper.ExtractHouseNumber(x.HouseNumber)),

            VotingCardSort.Name => voters.ThenBy(x => x.LastName)
                                         .ThenBy(x => x.FirstName),

            VotingCardSort.Place => voters.ThenBy(x => x.Town),

            VotingCardSort.Denomination => voters.ThenBy(x => x.IsMinor)
            .ThenBy(x => VoterTypeHelper.GetVoterTypeSortValue(x.VoterType))
            .ThenBy(x => DenominationHelper.GetDenominationSortValue(x.Religion)),

            VotingCardSort.Household => voters.ThenByDescending(x => x.IsHouseholder),

            _ => voters,
        };
    }

    internal static partial class HouseNumberHelper
    {
        public static int ExtractHouseNumber(string? houseNumber)
        {
            if (string.IsNullOrWhiteSpace(houseNumber))
            {
                return 0;
            }

            var match = HouseNumberRegex().Match(houseNumber.Trim());
            return match.Success ? int.Parse(match.Value) : 0;
        }

        [GeneratedRegex(@"^\d+")]
        internal static partial Regex HouseNumberRegex();
    }

    internal static class DenominationHelper
    {
        public static int GetDenominationSortValue(string? religion) =>
            religion == "111" ? 0 :
            religion == "121" ? 1 :
            religion == "122" ? 2 :
            religion == "211" ? 3 :
            religion == "211201" ? 4 :
            religion == "211301" ? 5 : 6;
    }

    internal static class VoterTypeHelper
    {
        public static int GetVoterTypeSortValue(VoterType voterType) =>
            voterType == VoterType.Swiss ? 0 :
            voterType == VoterType.SwissAbroad ? 1 :
            voterType == VoterType.Unspecified ? 2 :
            voterType == VoterType.Foreigner ? 3 : 4;
    }
}
