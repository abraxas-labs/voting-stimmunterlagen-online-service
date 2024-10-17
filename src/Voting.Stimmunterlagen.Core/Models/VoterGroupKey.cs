// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

internal class VoterGroupKey
{
    private readonly object?[] _groupValues;
    private readonly int _hashCode;

    public VoterGroupKey(Voter voter, IEnumerable<VotingCardGroup> groups)
    {
        _groupValues = BuildValues(voter, groups).ToArray();
        _hashCode = BuildHashCode(_groupValues);
    }

    public override bool Equals(object? obj)
        => obj is VoterGroupKey other && _groupValues.SequenceEqual(other._groupValues);

    public override int GetHashCode()
        => _hashCode;

    private static IEnumerable<object?> BuildValues(Voter voter, IEnumerable<VotingCardGroup> groups)
    {
        yield return voter.List!.VotingCardType;

        var hasGroup = false;
        foreach (var group in groups)
        {
            yield return voter.GetGroupValue(group);
            hasGroup = true;
        }

        if (!hasGroup)
        {
            yield return voter.Bfs;
        }
    }

    private static int BuildHashCode(object?[] values)
    {
        var hash = default(HashCode);

        foreach (var value in values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}
