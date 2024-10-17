// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class ContestDomainOfInfluenceHierarchyEntry : BaseEntity, IDomainOfInfluenceHierarchyEntry<ContestDomainOfInfluence>
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public Guid ParentDomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? ParentDomainOfInfluence { get; set; }
}
