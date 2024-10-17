// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class Template : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;

    public string InternName { get; set; } = string.Empty;

    public ICollection<TemplateDataContainer>? DataContainers { get; set; }

    public ICollection<ContestVotingCardLayout>? ContestVotingCardLayouts { get; set; }

    public ICollection<DomainOfInfluenceVotingCardLayout>? DomainOfInfluenceVotingCardLayouts { get; set; }

    public ICollection<DomainOfInfluenceVotingCardLayout>? OverriddenDomainOfInfluenceVotingCardLayouts { get; set; }
}
