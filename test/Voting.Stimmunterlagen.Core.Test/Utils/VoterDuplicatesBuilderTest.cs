// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;
using Xunit;

namespace Voting.Stimmunterlagen.Core.Test.Utils;

public class VoterDuplicatesBuilderTest
{
    private static readonly Guid SwissVoterListId = Guid.Parse("61a653ce-f672-4743-9027-068bb0f70851");
    private static readonly Guid SwissAbroadVoterListId = Guid.Parse("3aab6dfb-5382-48ff-a610-c3309b5e3964");
    private static readonly Guid EVotingVoterListId = Guid.Parse("eacfcd31-524b-4a84-99e6-db4b6b6ceb6d");

    private static readonly VoterData Duplicate1 = new("Max10", "Muster10", "2010", "Strasse", "1");
    private static readonly VoterData Duplicate2 = new("Max11", "Muster11", "2011", "Strasse", "1");
    private static readonly VoterData Duplicate3 = new("Max12", "Muster12", "2012", "Strasse", "1");
    private static readonly VoterData ExistingVoter1 = new("Max20", "Muster20", "2020", "Strasse", "1");

    [Fact]
    public void DuplicateVoterOnSameImportShouldBeMarkedAsInternalDuplicate()
    {
        var (import, builder) = GetImportAndBuilder();

        var results = new List<VoterDuplicatesBuilderNextVoterResult>
        {
            SetNextVoterOnBuilderAndReturnResult(builder, "Max", "Muster", "2000", "Strasse", "1", VotingCardType.Swiss),
            SetNextVoterOnBuilderAndReturnResult(builder, "Max", "Muster", "2000", "Strasse", "1", VotingCardType.SwissAbroad),
            SetNextVoterOnBuilderAndReturnResult(builder, "Max", "Muster", "2000", "Strasse", "1", VotingCardType.Swiss),
            SetNextVoterOnBuilderAndReturnResult(builder, "Max", "Muster", "2001", "Strasse", "1", VotingCardType.Swiss),
        };

        var expectedStateResults = new[]
        {
            VoterDuplicatesBuilderNextVoterResultState.NoActionRequired,
            VoterDuplicatesBuilderNextVoterResultState.InternalDuplicate,
            VoterDuplicatesBuilderNextVoterResultState.InternalDuplicate,
            VoterDuplicatesBuilderNextVoterResultState.NoActionRequired,
        };

        results.Select(r => r.State).SequenceEqual(expectedStateResults).Should().BeTrue();
        results.ShouldMatchSnapshot();
    }

    [Fact]
    public void DuplicateVoterOnSameDomainOfInfluenceShouldBeMarkedAsExternalDuplicate()
    {
        var (import, builder) = GetImportAndBuilder();

        var results = new List<VoterDuplicatesBuilderNextVoterResult>
        {
            SetNextVoterOnBuilderAndReturnResult(builder, Duplicate1.FirstName, Duplicate1.LastName, "2000", Duplicate1.Street, Duplicate1.HouseNumber, VotingCardType.Swiss),
            SetNextVoterOnBuilderAndReturnResult(builder, ExistingVoter1.FirstName, ExistingVoter1.LastName, ExistingVoter1.DateOfBirth, ExistingVoter1.Street, ExistingVoter1.HouseNumber, VotingCardType.EVoting),
            SetNextVoterOnBuilderAndReturnResult(builder, Duplicate1.FirstName, Duplicate1.LastName, Duplicate1.DateOfBirth, Duplicate1.Street, Duplicate1.HouseNumber, VotingCardType.Swiss),
            SetNextVoterOnBuilderAndReturnResult(builder, Duplicate2.FirstName, Duplicate2.LastName, Duplicate2.DateOfBirth, Duplicate2.Street, Duplicate2.HouseNumber, VotingCardType.EVoting),
            SetNextVoterOnBuilderAndReturnResult(builder, Duplicate3.FirstName, "Test", Duplicate3.DateOfBirth, Duplicate3.Street, Duplicate3.HouseNumber, VotingCardType.Swiss),
            SetNextVoterOnBuilderAndReturnResult(builder, Duplicate1.FirstName, Duplicate1.LastName, Duplicate1.DateOfBirth, Duplicate1.Street, Duplicate1.HouseNumber, VotingCardType.EVoting),
        };

        var expectedStateResults = new[]
        {
            VoterDuplicatesBuilderNextVoterResultState.NoActionRequired,
            VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateCreateRequired,
            VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateReferenceRequired,
            VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateReferenceRequired,
            VoterDuplicatesBuilderNextVoterResultState.NoActionRequired,
            VoterDuplicatesBuilderNextVoterResultState.InternalDuplicate,
        };

        results.Select(r => r.State).SequenceEqual(expectedStateResults).Should().BeTrue();
        results.ShouldMatchSnapshot();
    }

    private (VoterListImport VoterListImport, VoterDuplicatesBuilder VoterDuplicatesBuilder) GetImportAndBuilder()
    {
        var import = new VoterListImport
        {
            VoterLists = new List<VoterList>
            {
                new() { Id = SwissVoterListId, VotingCardType = VotingCardType.Swiss },
                new() { Id = SwissAbroadVoterListId, VotingCardType = VotingCardType.SwissAbroad },
                new() { Id = EVotingVoterListId, VotingCardType = VotingCardType.EVoting },
            },
            DomainOfInfluence = new()
            {
                VoterDuplicates = new List<DomainOfInfluenceVoterDuplicate>
                {
                    NewVoterDuplicate(
                        Guid.Parse("39224725-c1f1-4258-a4a2-7b51c1b6aff1"),
                        Duplicate1.FirstName,
                        Duplicate1.LastName,
                        Duplicate1.DateOfBirth,
                        Duplicate1.Street,
                        Duplicate1.HouseNumber,
                        new List<Guid> { Guid.Parse("83286209-8bd9-4d43-b3f2-48cd3b40012d"), Guid.Parse("ae5cbdbc-94b7-4a19-9b6b-41ad7f496290") }),
                    NewVoterDuplicate(
                        Guid.Parse("93d464a7-9d4b-4cea-8c51-cdd3c26086c9"),
                        Duplicate2.FirstName,
                        Duplicate2.LastName,
                        Duplicate2.DateOfBirth,
                        Duplicate2.Street,
                        Duplicate2.HouseNumber,
                        new List<Guid> { Guid.Parse("90773e76-bd47-4f1c-a5fe-21fde6ecfd48") }),
                    NewVoterDuplicate(
                        Guid.Parse("b8ffff17-b31e-452b-8536-9a2175b0335d"),
                        Duplicate3.FirstName,
                        Duplicate3.LastName,
                        Duplicate3.DateOfBirth,
                        Duplicate3.Street,
                        Duplicate3.HouseNumber,
                        new List<Guid> { Guid.Parse("d8181277-73d6-4b72-8381-fe50ff2cb683") }),
                },
            },
        };

        var voterItems = import.DomainOfInfluence.VoterDuplicates
            .SelectMany(d => d.Voters!)
            .Select(v => new { v.Id, Key = new VoterKey(v.FirstName, v.LastName, v.DateOfBirth, v.Street, v.HouseNumber) })
            .ToList();

        voterItems.Add(new
        {
            Id = Guid.Parse("0b76e2a4-c345-4aa3-a464-6f44424070be"),
            Key = new VoterKey(
                ExistingVoter1.FirstName,
                ExistingVoter1.LastName,
                ExistingVoter1.DateOfBirth,
                ExistingVoter1.Street,
                ExistingVoter1.HouseNumber),
        });

        var existingVoterKeys = voterItems
            .GroupBy(i => i.Key)
            .ToDictionary(i => i.Key, i => i.Select(k => k.Id).ToList());

        foreach (var voterDuplicate in import.DomainOfInfluence.VoterDuplicates)
        {
            voterDuplicate.Voters = null;
        }

        return (import, new VoterDuplicatesBuilder(import.DomainOfInfluence.Id, import.DomainOfInfluence.VoterDuplicates.ToList(), existingVoterKeys));
    }

    private VoterDuplicatesBuilderNextVoterResult SetNextVoterOnBuilderAndReturnResult(
        VoterDuplicatesBuilder builder,
        string firstName,
        string lastName,
        string dateOfBirth,
        string street,
        string? houseNumber,
        VotingCardType vcType)
    {
        var listId = vcType == VotingCardType.Swiss
            ? SwissVoterListId
            : vcType == VotingCardType.SwissAbroad
                ? SwissAbroadVoterListId
                : EVotingVoterListId;

        var voter = new Voter
        {
            PersonId = "1",
            PersonIdCategory = "1",
            FirstName = firstName,
            LastName = lastName,
            Street = street,
            HouseNumber = houseNumber,
            Sex = SexType.Undefined,
            DateOfBirth = dateOfBirth,
            ListId = listId,
            VotingCardType = vcType,
        };

        return builder.NextVoter(voter);
    }

    private DomainOfInfluenceVoterDuplicate NewVoterDuplicate(Guid id, string firstName, string lastName, string dateOfBirth, string street, string? houseNumber, List<Guid> existingVoterIds)
    {
        return new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Street = street,
            HouseNumber = houseNumber,
            Voters = existingVoterIds.ConvertAll(id => new Voter
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Street = street,
                HouseNumber = houseNumber,
                VoterDuplicateId = id,
            }),
        };
    }

    private record VoterData(string FirstName, string LastName, string DateOfBirth, string Street, string? HouseNumber);
}
