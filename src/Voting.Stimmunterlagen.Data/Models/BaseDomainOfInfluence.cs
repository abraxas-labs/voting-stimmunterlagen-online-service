// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public abstract class BaseDomainOfInfluence : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string AuthorityName { get; set; } = string.Empty;

    public string SecureConnectId { get; set; } = string.Empty;

    public bool ResponsibleForVotingCards { get; set; }

    public string Bfs { get; set; } = string.Empty;

    public DomainOfInfluenceType Type { get; set; }

    public DomainOfInfluenceCanton Canton { get; set; }

    public DomainOfInfluenceVotingCardReturnAddress? ReturnAddress { get; set; }

    public DomainOfInfluenceVotingCardPrintData? PrintData { get; set; }

    public DomainOfInfluenceVotingCardSwissPostData? SwissPostData { get; set; }

    public string? LogoRef { get; set; }

    public VotingCardColor VotingCardColor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the domain of influence uses an external printing center,
    /// which implies that attachments are not used (no <see cref="Step.Attachments"/>, except it is a contest manager).
    /// </summary>
    public bool ExternalPrintingCenter { get; set; }

    public string ExternalPrintingCenterEaiMessageType { get; set; } = string.Empty;

    public string SapCustomerOrderNumber { get; set; } = string.Empty;

    public DomainOfInfluenceCantonDefaults CantonDefaults { get; set; } = new();

    public Guid? ParentId { get; set; }

    public Guid RootId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether VOTING Stimmregister is enabled.
    /// </summary>
    public bool ElectoralRegistrationEnabled { get; set; }
}

public abstract class BaseDomainOfInfluence<T, THierarchyEntry> : BaseDomainOfInfluence
    where T : BaseDomainOfInfluence<T, THierarchyEntry>
    where THierarchyEntry : IDomainOfInfluenceHierarchyEntry<T>
{
    public T? Parent { get; set; }

    public T? Root { get; set; }

    public ICollection<T>? Children { get; set; }

    public ICollection<T>? RootOfChildrenAndSelf { get; set; }

    /// <summary>
    /// Gets or sets a collection where the current DomainOfInfluence is the parent. Child DomainOfInfluences are mapped
    /// to the DomainOfInfluence property of the hierarchy entry.
    /// </summary>
    public ICollection<THierarchyEntry>? ParentHierarchyEntries { get; set; }

    /// <summary>
    /// Gets or sets a collection where the current DomainOfInfluence is the child. Parent DomainOfInfluences are mapped
    /// to the ParentDomainOfInfluence property of the hierarchy entry.
    /// </summary>
    public ICollection<THierarchyEntry>? HierarchyEntries { get; set; }
}
