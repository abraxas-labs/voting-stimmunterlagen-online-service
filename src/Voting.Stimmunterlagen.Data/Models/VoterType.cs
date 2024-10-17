// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum VoterType
{
    /// <summary>
    /// Unspecified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Swiss domestic voter.
    /// </summary>
    Swiss,

    /// <summary>
    /// Swiss abroad voter.
    /// </summary>
    SwissAbroad,

    /// <summary>
    /// Foreigner voter.
    /// </summary>
    Foreigner,
}
