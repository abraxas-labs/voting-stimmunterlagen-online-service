// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Utils;

/// <summary>
/// A class which provides SQL SEQUENCE names.
/// </summary>
public static class SequenceNames
{
    private const string VoterContestIndexSequencePrefix = "VoterContestIndex";

    public static string BuildVoterContestIndexSequenceName(Guid contestId)
    {
        return $"{VoterContestIndexSequencePrefix}_{contestId}";
    }
}
