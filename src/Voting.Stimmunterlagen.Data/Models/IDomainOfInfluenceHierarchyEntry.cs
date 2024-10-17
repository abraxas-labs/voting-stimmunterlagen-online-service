// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public interface IDomainOfInfluenceHierarchyEntry<T>
{
    T? DomainOfInfluence { get; set; }

    Guid DomainOfInfluenceId { get; set; }

    Guid ParentDomainOfInfluenceId { get; set; }

    T? ParentDomainOfInfluence { get; set; }
}
