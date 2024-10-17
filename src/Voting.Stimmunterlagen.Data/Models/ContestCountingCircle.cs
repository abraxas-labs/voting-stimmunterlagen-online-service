// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestCountingCircle : BaseCountingCircle, IHasContest
{
    public Contest? Contest { get; set; }

    public Guid ContestId { get; set; }

    // The ID of the VOTING Basis CountingCircle (the ID of the "live" counting circle)
    // Don't use reference integrity here since we want to preserve this ID (in case the "live" CC gets deleted)
    public Guid BasisCountingCircleId { get; set; }

    public ICollection<ContestDomainOfInfluenceCountingCircle>? DomainOfInfluences { get; set; }
}
