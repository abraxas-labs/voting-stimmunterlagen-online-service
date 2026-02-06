// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Stimmunterlagen.Core.Models.VoterListImport;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Utils;

public enum VoterDuplicatesBuilderNextVoterResultState
{
    Unspecified,
    NoActionRequired,
    InternalDuplicate,
    ExternalDuplicateCreateRequired,
    ExternalDuplicateReferenceRequired,
}

public class VoterDuplicatesBuilder
{
    private readonly Guid _domainOfInfluenceId;
    private readonly HashSet<VoterKey> _existingExternalVoterKeys = new();
    private readonly HashSet<VoterKey> _existingInternalVoterKeys = new();
    private readonly List<DomainOfInfluenceVoterDuplicate> _existingVoterDuplicates = new();
    private readonly Dictionary<VoterKey, List<Guid>> _existingVoterIdsByVoterKey = new();

    public VoterDuplicatesBuilder(Guid domainOfInfluenceId, List<DomainOfInfluenceVoterDuplicate> existingVoterDuplicates, Dictionary<VoterKey, List<Guid>> existingVoterIdsByVoterKey)
    {
        _domainOfInfluenceId = domainOfInfluenceId;
        _existingVoterDuplicates.AddRange(existingVoterDuplicates);
        _existingExternalVoterKeys.AddRange(existingVoterIdsByVoterKey.Keys);
        _existingVoterIdsByVoterKey.AddRange(existingVoterIdsByVoterKey);
    }

    public VoterDuplicatesBuilderNextVoterResult NextVoter(Voter voter)
    {
        var voterKey = BuildVoterKey(voter);

        // Internal (= same Import) duplicate check.
        if (!_existingInternalVoterKeys.Contains(voterKey))
        {
            _existingInternalVoterKeys.Add(voterKey);
        }
        else
        {
            return new VoterDuplicatesBuilderNextVoterResult(VoterDuplicatesBuilderNextVoterResultState.InternalDuplicate);
        }

        // External (= same Domain Of Influence) duplicate check
        if (!_existingExternalVoterKeys.Contains(voterKey))
        {
            _existingExternalVoterKeys.Add(voterKey);
            return new VoterDuplicatesBuilderNextVoterResult(VoterDuplicatesBuilderNextVoterResultState.NoActionRequired);
        }

        var existingVoterIds = _existingVoterIdsByVoterKey[voterKey]!;

        var existingVoterDuplicate = GetVoterDuplicate(voterKey);
        if (existingVoterDuplicate != null)
        {
            return new VoterDuplicatesBuilderNextVoterResult(
                VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateReferenceRequired,
                new VoterDuplicatesBuilderVoterDuplicateData(existingVoterDuplicate, existingVoterIds));
        }

        var newVoterDuplicate = MapToVoterDuplicate(voterKey);
        _existingVoterDuplicates.Add(newVoterDuplicate);
        return new VoterDuplicatesBuilderNextVoterResult(
            VoterDuplicatesBuilderNextVoterResultState.ExternalDuplicateCreateRequired,
            new VoterDuplicatesBuilderVoterDuplicateData(newVoterDuplicate, existingVoterIds));
    }

    private DomainOfInfluenceVoterDuplicate MapToVoterDuplicate(VoterKey voter)
    {
        return new()
        {
            FirstName = voter.FirstName,
            LastName = voter.LastName,
            DateOfBirth = voter.DateOfBirth,
            Street = voter.Street,
            HouseNumber = voter.HouseNumber,
            DomainOfInfluenceId = _domainOfInfluenceId,
        };
    }

    private DomainOfInfluenceVoterDuplicate? GetVoterDuplicate(VoterKey voterKey)
    {
        return _existingVoterDuplicates.Find(d =>
            d.FirstName == voterKey.FirstName
            && d.LastName == voterKey.LastName
            && d.DateOfBirth == voterKey.DateOfBirth
            && d.Street == voterKey.Street
            && d.HouseNumber == voterKey.HouseNumber);
    }

    private VoterKey BuildVoterKey(Voter voter) => new VoterKey(voter.FirstName, voter.LastName, voter.DateOfBirth, voter.Street, voter.HouseNumber);
}

public record VoterDuplicatesBuilderNextVoterResult(VoterDuplicatesBuilderNextVoterResultState State, VoterDuplicatesBuilderVoterDuplicateData? Data = null);

public record VoterDuplicatesBuilderVoterDuplicateData(DomainOfInfluenceVoterDuplicate VoterDuplicate, List<Guid> ExistingExternalVoterIds);
