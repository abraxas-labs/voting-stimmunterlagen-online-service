// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using FluentAssertions;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class VoterHouseholdBuilderTest
{
    private static readonly Guid SwissVoterListId = Guid.Parse("61a653ce-f672-4743-9027-068bb0f70851");
    private static readonly Guid SwissAbroadVoterListId = Guid.Parse("3aab6dfb-5382-48ff-a610-c3309b5e3964");
    private static readonly Guid EVotingVoterListId = Guid.Parse("eacfcd31-524b-4a84-99e6-db4b6b6ceb6d");

    [Fact]
    public void EachApartmentShouldHaveAnAssignedVoter()
    {
        var builder = GetBuilder();
        SetNextVoterOnBuilder(builder, "1", "Max", "Householder", true, 1, 1, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "2", "Maxima", "Muster", false, 1, 2, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "3", "Maximus", "Muster", false, 1, 2, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "4", "No", "Building", true, null, 3, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "5", "No", "Apartment", true, 1, null, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "6", "No", "Residence", true, null, null, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "7", "Fritz", "Meier", true, 2, 1, true, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "8", "Heidi", "Meier", false, 2, 1, false, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "9", "Birgit", "Müller", false, 2, 2, true, SwissVoterListId);
        SetNextVoterOnBuilder(builder, "10", "Hans", "Müller", false, 2, 2, true, SwissVoterListId);

        var householdsByList = builder.GetHouseholdsByListId();

        var swissVoterList = householdsByList[SwissVoterListId];
        swissVoterList.Values.Count.Should().Be(3);
        swissVoterList[(1, 1)].PersonId.Should().Be("1");
        swissVoterList[(1, 1)].IsHouseholder.Should().BeTrue();
        swissVoterList[(1, 2)].PersonId.Should().Be("2");
        swissVoterList[(1, 2)].IsHouseholder.Should().BeFalse();
        swissVoterList[(2, 1)].PersonId.Should().Be("8");
        swissVoterList[(2, 1)].IsHouseholder.Should().BeFalse();

        var swissAbroadVoterList = householdsByList[SwissAbroadVoterListId];
        swissAbroadVoterList.Count.Should().Be(0);

        var eVotingVoterList = householdsByList[EVotingVoterListId];
        eVotingVoterList.Count.Should().Be(0);
    }

    private VoterHouseholdBuilder GetBuilder()
    {
        var import = new VoterListImport
        {
            VoterLists = new List<VoterList>
            {
                new() { Id = SwissVoterListId },
                new() { Id = SwissAbroadVoterListId },
                new() { Id = EVotingVoterListId },
            },
        };

        return new VoterHouseholdBuilder(import);
    }

    private void SetNextVoterOnBuilder(
        VoterHouseholdBuilder builder,
        string personId,
        string firstName,
        string lastName,
        bool isHouseholder,
        int? residenceBuildingId,
        int? residenceApartmentId,
        bool sendVotingCardsToDomainOfInfluenceReturnAddress,
        Guid listId)
    {
        builder.NextVoter(new Voter
        {
            PersonId = personId,
            FirstName = firstName,
            LastName = lastName,
            IsHouseholder = isHouseholder,
            ResidenceBuildingId = residenceBuildingId,
            ResidenceApartmentId = residenceApartmentId,
            SendVotingCardsToDomainOfInfluenceReturnAddress = sendVotingCardsToDomainOfInfluenceReturnAddress,
            ListId = listId,
        });
    }
}
