// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.Extensions;

public static class DomainOfInfluenceVotingCardLayoutExtensions
{
    public static bool IsA4Template(this DomainOfInfluenceVotingCardLayout layout)
    {
        EnsureTemplateNotNull(layout);
        return !layout.EffectiveTemplate!.InternName.Contains("_a5");
    }

    public static bool IsA5Template(this DomainOfInfluenceVotingCardLayout layout)
    {
        EnsureTemplateNotNull(layout);
        return layout.EffectiveTemplate!.InternName.Contains("_a5");
    }

    public static bool IsDuplexTemplate(this DomainOfInfluenceVotingCardLayout layout)
    {
        EnsureTemplateNotNull(layout);
        return layout.EffectiveTemplate!.InternName.Contains("_duplex");
    }

    private static void EnsureTemplateNotNull(this DomainOfInfluenceVotingCardLayout layout)
    {
        if (layout.EffectiveTemplate == null)
        {
            throw new InvalidOperationException($"{nameof(layout.EffectiveTemplate)} must not be null");
        }
    }
}
