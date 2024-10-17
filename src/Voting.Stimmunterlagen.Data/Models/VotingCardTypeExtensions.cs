// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public static class VotingCardTypeExtensions
{
    public static bool OfflineGenerationRequired(this VotingCardType t)
        => t == VotingCardType.EVoting;
}
