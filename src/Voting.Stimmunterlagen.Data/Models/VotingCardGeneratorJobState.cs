// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum VotingCardGeneratorJobState
{
    Unspecified,
    Ready,
    Running,
    Completed,
    Failed,
    ReadyToRunOffline,
}
