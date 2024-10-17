// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Manager;

public class VotingCardGeneratorJobBuilderTest
{
    [Fact]
    public void GroupVotersShouldWork()
    {
        var list1 = new VoterList { VotingCardType = VotingCardType.Swiss };
        var list2 = new VoterList { VotingCardType = VotingCardType.EVoting };
        var grouped = VotingCardGeneratorJobBuilder.GroupVoters(
            new[]
            {
                    new Voter { FirstName = "0", Bfs = "1", LanguageOfCorrespondence = "de", List = list1 },
                    new Voter { FirstName = "0", Bfs = "1", LanguageOfCorrespondence = "de", List = list1 },
                    new Voter { FirstName = "0", Bfs = "2", LanguageOfCorrespondence = "de", List = list1 },
                    new Voter { FirstName = "1", Bfs = "2", LanguageOfCorrespondence = "en", SwissZipCode = 1000, List = list1 },
                    new Voter { FirstName = "2", Bfs = "2", LanguageOfCorrespondence = "en", SwissZipCode = 1001, List = list1 },
                    new Voter { FirstName = "3", Bfs = "3", LanguageOfCorrespondence = "fr", List = list1 },
                    new Voter { FirstName = "4", Bfs = "3", LanguageOfCorrespondence = "fr", List = list2 },
            },
            new[]
            {
                    VotingCardGroup.Language,
                    VotingCardGroup.ShippingRegion,
            })
            .ToList();

        grouped.Should().HaveCount(5);

        grouped
            .Select((group, i) => (group, i))
            .All(gi => gi.group.All(v => v.FirstName == gi.i.ToString()))
            .Should()
            .BeTrue();
    }

    [Fact]
    public void GroupVotersWithoutGroupShouldGroupByCardTypeAndBfs()
    {
        var list1 = new VoterList { VotingCardType = VotingCardType.Swiss };
        var list2 = new VoterList { VotingCardType = VotingCardType.EVoting };
        var grouped = VotingCardGeneratorJobBuilder.GroupVoters(
                new[]
                {
                        new Voter { FirstName = "0", Bfs = "1", LanguageOfCorrespondence = "de", List = list1 },
                        new Voter { FirstName = "0", Bfs = "1", LanguageOfCorrespondence = "de", List = list1 },
                        new Voter { FirstName = "1", Bfs = "1", LanguageOfCorrespondence = "de", List = list2 },
                        new Voter { FirstName = "2", Bfs = "2", LanguageOfCorrespondence = "en", List = list2 },
                        new Voter { FirstName = "2", Bfs = "2", LanguageOfCorrespondence = "en", List = list2 },
                },
                Enumerable.Empty<VotingCardGroup>())
            .ToList();

        grouped.Should().HaveCount(3);
        grouped
            .Select((group, i) => (group, i))
            .All(gi => gi.group.All(v => v.FirstName == gi.i.ToString()))
            .Should()
            .BeTrue();
    }
}
