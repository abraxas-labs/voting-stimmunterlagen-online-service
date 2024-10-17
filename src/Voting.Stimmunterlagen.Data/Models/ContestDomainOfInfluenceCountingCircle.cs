// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestDomainOfInfluenceCountingCircle : BaseEntity
{
    public Guid DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid CountingCircleId { get; set; }

    public ContestCountingCircle? CountingCircle { get; set; }
}
