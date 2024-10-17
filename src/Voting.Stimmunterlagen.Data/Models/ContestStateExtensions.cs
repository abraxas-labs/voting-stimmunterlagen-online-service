// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public static class ContestStateExtensions
{
    public static bool TestingPhaseEnded(this ContestState state) => state > ContestState.TestingPhase;
}
