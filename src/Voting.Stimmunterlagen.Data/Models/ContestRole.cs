// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum ContestRole
{
    /// <summary>
    /// This domain of influence doesn't have any role on this contest.
    /// </summary>
    None,

    /// <summary>
    /// The manager of this contest.
    /// </summary>
    Manager,

    /// <summary>
    /// An attendee of this contest (can be responsible for political businesses inside this contest).
    /// </summary>
    Attendee,
}
