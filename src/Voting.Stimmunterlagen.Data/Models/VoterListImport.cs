// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class VoterListImport : BaseEntity, IHasContestDomainOfInfluence
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public string SourceId { get; set; } = string.Empty;

    public VoterListSource Source { get; set; }

    public bool AutoSendVotingCardsToDomainOfInfluenceReturnAddressSplit { get; set; }

    public DateTime LastUpdate { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<VoterList>? VoterLists { get; set; }
}
