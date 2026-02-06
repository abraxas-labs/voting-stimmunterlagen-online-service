// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snapper;
using Voting.Stimmunterlagen.Core.Extensions;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Extensions;

public class VoterAsyncEnummerableExtensionTest
{
    [Fact]
    public async Task SortOfStreetShouldWork()
    {
        var voterList = await GetVotersAsync().OrderBySortingCriteriaAsync([VotingCardSort.Street]).Select(v => new { v.FirstName, v.LastName, v.Street, v.HouseNumber }).ToListAsync();
        voterList.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task SortDenominationShouldWork()
    {
        var voterList = await GetVotersAsync().OrderBySortingCriteriaAsync([VotingCardSort.Denomination]).Select(v => new { v.FirstName, v.LastName, v.Religion, v.VoterType, v.IsMinor }).ToListAsync();
        voterList.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task SortOfDenominationAndStreetShouldWork()
    {
        var voterList = await GetVotersAsync().OrderBySortingCriteriaAsync([VotingCardSort.Denomination, VotingCardSort.Street]).Select(v => new { v.FirstName, v.LastName, v.Street, v.HouseNumber, v.Religion, v.VoterType, v.IsMinor }).ToListAsync();
        voterList.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task SortOfStreetAndDenominationShouldWork()
    {
        var voterList = await GetVotersAsync().OrderBySortingCriteriaAsync([VotingCardSort.Street, VotingCardSort.Denomination]).Select(v => new { v.FirstName, v.LastName, v.Street, v.HouseNumber, v.Religion, v.VoterType, v.IsMinor }).ToListAsync();
        voterList.ShouldMatchSnapshot();
    }

    private static Voter BuildVoter(int index, VoterType voterType, bool isMinor, string? religionCode, string street, string houseNumber)
    {
        return new Voter
        {
            FirstName = "FirstName" + index.ToString(),
            LastName = "LastName" + index.ToString(),
            VoterType = voterType,
            IsMinor = isMinor,
            Religion = religionCode,
            Street = street,
            HouseNumber = houseNumber,
        };
    }

    private async IAsyncEnumerable<Voter> GetVotersAsync()
    {
        yield return BuildVoter(1, VoterType.Foreigner, false, "211301", "Hauptstrasse", "1A");
        yield return BuildVoter(2, VoterType.Foreigner, true, "211301", "Hauptstrasse", "10 B");
        yield return BuildVoter(3, VoterType.Swiss, true, "111", "Hauptstrasse", "2");
        yield return BuildVoter(4, VoterType.Swiss, false, "211301", "Hauptstrasse", "100");
        yield return BuildVoter(5, VoterType.Swiss, false, "211", "Auweg", "100");
        yield return BuildVoter(6, VoterType.Swiss, false, "211", "Auweg", "2");
        yield return BuildVoter(7, VoterType.Swiss, false, "111", "Zilstrasse", "100");
        yield return BuildVoter(8, VoterType.Unspecified, false, "211301", "Zilstrasse", "1");
        yield return BuildVoter(9, VoterType.SwissAbroad, false, "122", "Zilstrasse", "2");

        await Task.Yield();
    }
}
