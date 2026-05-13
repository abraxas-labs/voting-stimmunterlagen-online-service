// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;

namespace Voting.Stimmunterlagen.Data.Models;

public class DomainOfInfluenceVotingCardLayout : VotingCardLayout, IHasContestDomainOfInfluence
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public int? DomainOfInfluenceTemplateId { get; set; }

    public Template? DomainOfInfluenceTemplate { get; set; }

    public int? OverriddenTemplateId { get; set; }

    public Template? OverriddenTemplate { get; set; }

    public Template? EffectiveTemplate => OverriddenTemplate ?? DomainOfInfluenceTemplate ?? Template;

    public int? EffectiveTemplateId => OverriddenTemplateId ?? DomainOfInfluenceTemplateId ?? TemplateId;

    public ICollection<TemplateDataFieldValue>? TemplateDataFieldValues { get; set; }

    public ICollection<VotingCardGeneratorJob> Jobs { get; set; } = new HashSet<VotingCardGeneratorJob>();

    public ICollection<ManualVotingCardGeneratorJob> ManualJobs { get; set; } = new HashSet<ManualVotingCardGeneratorJob>();
}
