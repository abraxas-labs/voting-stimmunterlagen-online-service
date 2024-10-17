// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.EVoting.Models;

public class Contest
{
    public DateTime Date { get; set; }

    public IReadOnlyCollection<DomainOfInfluence>? ContestDomainOfInfluences { get; set; }
}
