// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Data.Models;

public class CountingCircle : BaseCountingCircle
{
    public ICollection<DomainOfInfluenceCountingCircle>? DomainOfInfluences { get; set; }
}
