// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Data.Models;

public class AdditionalInvoicePosition : AuditedEntity, IHasContestDomainOfInfluence
{
    public string MaterialNumber { get; set; } = string.Empty;

    public decimal Amount => AmountCentime / 100M;

    public int AmountCentime { get; set; }

    public string Comment { get; set; } = string.Empty;

    public Guid DomainOfInfluenceId { get; set; }

    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }
}
