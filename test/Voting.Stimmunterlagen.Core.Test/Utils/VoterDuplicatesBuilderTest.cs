// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class VoterDuplicatesBuilderTest
{
    private static readonly Guid SwissVoterListId = Guid.Parse("61a653ce-f672-4743-9027-068bb0f70851");
    private static readonly Guid SwissAbroadVoterListId = Guid.Parse("3aab6dfb-5382-48ff-a610-c3309b5e3964");
    private static readonly Guid EVotingVoterListId = Guid.Parse("eacfcd31-524b-4a84-99e6-db4b6b6ceb6d");

    [Fact]
    public void UniqueVotersPerListOnSameImportShouldHaveNoDuplicates()
    {
        var (import, builder) = GetImportAndBuilder();
        SetNextVoterOnBuilder(builder, "1", "Ellena", "Glass", SexType.Female, "1981", VotingCardType.Swiss);
        SetNextVoterOnBuilder(builder, "1", "Ellena", "Glass", SexType.Female, "1981", VotingCardType.SwissAbroad);
        SetNextVoterOnBuilder(builder, "1", "Ellena", "Glass", SexType.Female, "1981", VotingCardType.EVoting);
        import.VoterLists!.SelectMany(vl => vl.VoterDuplicates!).Any().Should().BeFalse();
    }

    [Fact]
    public void DuplicatePersonIdsOnSameListShouldBeIncluded()
    {
        var (import, builder) = GetImportAndBuilder();
        SetNextVoterOnBuilder(builder, "1", "Ellena", "Glass", SexType.Female, "1981", VotingCardType.Swiss);
        SetNextVoterOnBuilder(builder, "2", "Maximillian", "Franklin", SexType.Male, "1992", VotingCardType.Swiss);
        SetNextVoterOnBuilder(builder, "1", "Alexis", "Jackson", SexType.Male, "2000", VotingCardType.Swiss);
        SetNextVoterOnBuilder(builder, "1", "Sion", "Cohen", SexType.Male, "2003", VotingCardType.Swiss);

        import.VoterLists!.Where(vl => vl.Id != SwissVoterListId).SelectMany(vl => vl.VoterDuplicates!).Any().Should().BeFalse();
        var swissDuplicates = import.VoterLists!.Single(vl => vl.Id == SwissVoterListId).VoterDuplicates!.ToList();

        swissDuplicates.MatchSnapshot();
    }

    private (VoterListImport VoterListImport, VoterDuplicatesBuilder VoterDuplicatesBuilder) GetImportAndBuilder()
    {
        var import = new VoterListImport
        {
            VoterLists = new List<VoterList>
            {
                new() { Id = SwissVoterListId, VotingCardType = VotingCardType.Swiss, VoterDuplicates = new List<VoterDuplicate>() },
                new() { Id = SwissAbroadVoterListId, VotingCardType = VotingCardType.SwissAbroad, VoterDuplicates = new List<VoterDuplicate>() },
                new() { Id = EVotingVoterListId, VotingCardType = VotingCardType.EVoting, VoterDuplicates = new List<VoterDuplicate>() },
            },
        };

        return (import, new VoterDuplicatesBuilder(import));
    }

    private void SetNextVoterOnBuilder(
        VoterDuplicatesBuilder builder,
        string personId,
        string firstName,
        string lastName,
        SexType sex,
        string dateOfBirth,
        VotingCardType vcType)
    {
        var listId = vcType == VotingCardType.Swiss
            ? SwissVoterListId
            : vcType == VotingCardType.SwissAbroad
                ? SwissAbroadVoterListId
                : EVotingVoterListId;

        builder.NextVoter(new Voter
        {
            PersonId = personId,
            FirstName = firstName,
            LastName = lastName,
            Sex = sex,
            DateOfBirth = dateOfBirth,
            ListId = listId,
            VotingCardType = vcType,
        });
    }
}
