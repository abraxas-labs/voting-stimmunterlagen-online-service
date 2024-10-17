// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluence : BaseDomainOfInfluence<DomainOfInfluence, DomainOfInfluenceHierarchyEntry>
{
    public ICollection<DomainOfInfluenceCountingCircle>? CountingCircles { get; set; }
}
