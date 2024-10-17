// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum ContestState
{
    /// <summary>
    /// Unspecified contest state.
    /// </summary>
    Unspecified,

    /// <summary>
    /// contest is in testing phase.
    /// Skip value 0 since it is the undefined value in proto.
    /// </summary>
    TestingPhase = 1,

    /// <summary>
    /// contest takes place in the future or today, but is not in the testing phase anymore.
    /// </summary>
    Active,

    /// <summary>
    /// contest has taken place in the past and is locked.
    /// </summary>
    PastLocked,

    /// <summary>
    /// contest has taken place in the past and is unlocked, but it will automatically get locked after the day ends.
    /// </summary>
    PastUnlocked,

    /// <summary>
    /// contest is archived.
    /// </summary>
    Archived,
}
