// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Ech0045_4_0;
using FluentAssertions;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Mapping.V4;
using Xunit;

namespace Voting.Stimmunterlagen.Ech.Test.EchMappingTests;

public class VoterMappingTest
{
    [Fact]
    public void EchSwissVoterShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Swiss = new SwissDomesticType() }, false);
        AssertVotingCardType(voter, false, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchSwissAbroadVoterShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { SwissAbroad = new SwissAbroadType() }, false);
        AssertVotingCardType(voter, false, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchForeignVoterShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Foreigner = new ForeignerType() }, false);
        AssertVotingCardType(voter, false, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchSwissVoterWithEVoterFlagShouldConvertToEVotingVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Swiss = new SwissDomesticType() }, true);
        AssertVotingCardType(voter, false, VotingCardType.EVoting, true);
    }

    [Fact]
    public void EchSwissAbroadVoterWithEVoterFlagShouldConvertToEVotingVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { SwissAbroad = new SwissAbroadType() }, true);
        AssertVotingCardType(voter, false, VotingCardType.EVoting, true);
    }

    [Fact]
    public void EchForeignVoterWithEVoterFlagShouldConvertToEVotingVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Foreigner = new ForeignerType() }, true);
        AssertVotingCardType(voter, false, VotingCardType.EVoting, true);
    }

    [Fact]
    public void EVoterWithSendVotingCardsToReturnDomainOfInfluenceAddressShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Foreigner = new ForeignerType() }, true);
        AssertVotingCardType(voter, true, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchSwissVoterWithEVoterFlagAndEVotingDisabledShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Swiss = new SwissDomesticType() }, true);
        AssertVotingCardType(voter, true, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchSwissAbroadVoterWithEVoterFlagAndEVotingDisabledShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { SwissAbroad = new SwissAbroadType() }, true);
        AssertVotingCardType(voter, false, VotingCardType.Swiss, false);
    }

    [Fact]
    public void EchForeignVoterWithEVoterFlagAndEVotingDisabledShouldConvertToSwissVotingCard()
    {
        var voter = BuildVoter(new VotingPersonTypePerson { Foreigner = new ForeignerType() }, true);
        AssertVotingCardType(voter, false, VotingCardType.Swiss, false);
    }

    private void AssertVotingCardType(VotingPersonType votingPerson, bool sendVotingCardsToReturnDomainOfInfluenceAddress, VotingCardType expectedVotingCardType, bool eVotingEnabled)
    {
        VoterMapping.GetVotingCardType(votingPerson, sendVotingCardsToReturnDomainOfInfluenceAddress, eVotingEnabled).Should().Be(expectedVotingCardType);
    }

    private VotingPersonType BuildVoter(VotingPersonTypePerson person, bool isEVoter)
    {
        return new VotingPersonType()
        {
            Person = person,
            IsEvoter = isEVoter,
        };
    }
}
