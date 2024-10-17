// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Common;

namespace Voting.Stimmunterlagen.Core.Utils;

/// <summary>
/// Can be used for cases where deterministic id's are required.
/// </summary>
internal static class StimmunterlagenUuidV5
{
    private const string VotingBasisSeparator = ":";

    private static readonly Guid VotingStimmunterlagenContestDomainOfInfluenceNamespace = Guid.Parse("652ec40d-4b9c-4b48-8e59-978632b459e0");
    private static readonly Guid VotingStimmunterlagenContestCountingCircleNamespace = Guid.Parse("4da49a34-0df8-4ff2-b9e0-0773dcb55c97");

    internal static Guid BuildContestDomainOfInfluence(Guid contestId, Guid domainOfInfluenceId)
        => Create(VotingStimmunterlagenContestDomainOfInfluenceNamespace, contestId, domainOfInfluenceId);

    internal static Guid BuildContestCountingCircle(Guid contestId, Guid countingCircleId)
        => Create(VotingStimmunterlagenContestCountingCircleNamespace, contestId, countingCircleId);

    private static Guid Create(Guid ns, params Guid[] existingGuids)
    {
        return UuidV5.Create(
            ns,
            string.Join(
                VotingBasisSeparator,
                existingGuids));
    }
}
