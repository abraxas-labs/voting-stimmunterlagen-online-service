// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Voting.Stimmunterlagen.Data.FilterExpressions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class EVotingDomainOfInfluenceEntry
{
    public EVotingDomainOfInfluenceEntry(ContestDomainOfInfluence domainOfInfluence)
    {
        DomainOfInfluence = domainOfInfluence;
    }

    public ContestDomainOfInfluence DomainOfInfluence { get; }

    public bool EVotingReady => ContestDomainOfInfluenceFilterExpressions.InEVotingExportFilter.Compile()(DomainOfInfluence)
        && DomainOfInfluence.StepStates!.Any(s => s is { Step: Step.GenerateVotingCards, Approved: true });

    public int OwnPoliticalBusinessesCount => GetPoliticalBusinessPermissionCountByRole(PoliticalBusinessRole.Manager, false);

    // should always be greater or equal 0, because a manager is always also an attendee.
    public int ParentPoliticalBusinessesCount => GetPoliticalBusinessPermissionCountByRole(PoliticalBusinessRole.Attendee, true);

    public int CountOfVotingCardsForEVoters => DomainOfInfluence.VoterLists!.Where(vl => vl.VotingCardType == VotingCardType.EVoting).Sum(vl => vl.CountOfVotingCards);

    private int GetPoliticalBusinessPermissionCountByRole(PoliticalBusinessRole role, bool exceptOwnPoliticalBusinesses)
    {
        var ownPoliticalBusinessIds = DomainOfInfluence.PoliticalBusinessPermissionEntries!.Where(e => e.Role == PoliticalBusinessRole.Manager).Select(e => e.PoliticalBusinessId);
        return DomainOfInfluence.PoliticalBusinessPermissionEntries!.Count(e => e.Role == role && (!exceptOwnPoliticalBusinesses || !ownPoliticalBusinessIds.Contains(e.PoliticalBusinessId)));
    }
}
