// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluenceCountingCircle : BaseEntity
{
    public Guid DomainOfInfluenceId { get; set; }

    public DomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid CountingCircleId { get; set; }

    public CountingCircle? CountingCircle { get; set; }
}
