// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class GroupedDomainOfInfluenceVotingCardLayouts
{
    public GroupedDomainOfInfluenceVotingCardLayouts(
        ContestDomainOfInfluence domainOfInfluence,
        List<DomainOfInfluenceVotingCardLayout> layouts)
    {
        DomainOfInfluence = domainOfInfluence;
        Layouts = layouts;
    }

    public ContestDomainOfInfluence DomainOfInfluence { get; set; }

    public List<DomainOfInfluenceVotingCardLayout> Layouts { get; set; }
}
