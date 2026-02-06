// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class VoterPrintInfoAggregatorTest
{
    [Fact]
    public void TestWithoutDuplicatesShouldWork()
    {
        var voterA = BuildVoter("A", false, (DomainOfInfluenceType.Ch, "1"), (DomainOfInfluenceType.Ct, "1"));

        VoterPrintInfoAggregator.Aggregate(new[] { voterA }, Array.Empty<DomainOfInfluenceVoterDuplicate>());
        voterA.DomainOfInfluences!.Select(doi => doi.DomainOfInfluenceIdentification).SequenceEqual(new[]
        {
            "1",
            "1",
        }).Should().BeTrue();
    }

    [Fact]
    public void TestShouldWork()
    {
        var voterA = BuildVoter("A", false, (DomainOfInfluenceType.Ch, "1"));
        var voterB = BuildVoter("B", false, (DomainOfInfluenceType.Ch, "1"), (DomainOfInfluenceType.Ct, "1"));

        var voterADuplicate1 = BuildVoter("A", true, (DomainOfInfluenceType.Ch, "1"), (DomainOfInfluenceType.Sc, "SC"));
        var voterADuplicate2 = BuildVoter("A", true, (DomainOfInfluenceType.Ct, "1"), (DomainOfInfluenceType.Ki, "KI"));
        var voterADuplicate3 = BuildVoter("A", true, (DomainOfInfluenceType.Ct, "1"), (DomainOfInfluenceType.Ki, "KI"));
        var voterBDuplicate1 = BuildVoter("B", true, (DomainOfInfluenceType.Bz, "1"), (DomainOfInfluenceType.Ki, "KI"));

        var voterADoiDuplicate = BuildDomainOfInfluenceDuplicate(voterA, voterADuplicate1, voterADuplicate2, voterADuplicate3);
        var voterBDoiDuplicate = BuildDomainOfInfluenceDuplicate(voterB, voterBDuplicate1);

        VoterPrintInfoAggregator.Aggregate(new[] { voterA, voterB }, new[] { voterADoiDuplicate, voterBDoiDuplicate });
        voterA.DomainOfInfluences!.Select(doi => doi.DomainOfInfluenceIdentification).SequenceEqual(new[]
        {
            "1",
            "1",
            "SC",
            "KI",
        }).Should().BeTrue();
        voterB.DomainOfInfluences!.Select(doi => doi.DomainOfInfluenceIdentification).SequenceEqual(new[]
        {
            "1",
            "1",
            "1",
            "KI",
        }).Should().BeTrue();
    }

    private Voter BuildVoter(string name, bool isDuplicate, params (DomainOfInfluenceType Type, string Id)[] dois)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            FirstName = name,
            LastName = name,
            DomainOfInfluences = dois.Select(doi => new VoterDomainOfInfluence
            {
                DomainOfInfluenceIdentification = doi.Id,
                DomainOfInfluenceType = doi.Type,
            }).ToList(),
            VotingCardPrintDisabled = isDuplicate,
        };
    }

    private DomainOfInfluenceVoterDuplicate BuildDomainOfInfluenceDuplicate(params Voter[] voters)
    {
        var voter = voters[0];

        return new()
        {
            FirstName = voter.FirstName,
            LastName = voter.LastName,
            Voters = voters,
        };
    }
}
