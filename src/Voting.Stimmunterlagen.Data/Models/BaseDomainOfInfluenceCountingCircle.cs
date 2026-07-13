// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class BaseDomainOfInfluenceCountingCircle : BaseEntity
{
    public Guid DomainOfInfluenceId { get; set; }

    public Guid CountingCircleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the source domain of influence to which counting circle was assigned.
    /// </summary>
    public Guid SourceDomainOfInfluenceId { get; set; }

    public bool Inherited => DomainOfInfluenceId != SourceDomainOfInfluenceId;
}
