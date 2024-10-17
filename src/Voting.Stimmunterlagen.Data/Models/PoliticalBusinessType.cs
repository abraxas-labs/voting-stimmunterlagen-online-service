// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum PoliticalBusinessType
{
    Unspecified,

    /// <summary>
    /// A vote.
    /// </summary>
    Vote,

    /// <summary>
    /// A majority election.
    /// </summary>
    MajorityElection,

    /// <summary>
    /// A proportional election.
    /// </summary>
    ProportionalElection,

    /// <summary>
    /// A secondary majority election.
    /// </summary>
    SecondaryMajorityElection,
}
