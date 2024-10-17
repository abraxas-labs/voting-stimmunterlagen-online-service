// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

public abstract class VotingCardLayoutBuilder<T>
    where T : VotingCardLayout, new()
{
    protected void SyncVotingCardLayouts(ICollection<T> votingCardLayouts)
    {
        var layoutsByType = votingCardLayouts.ToDictionary(x => x.VotingCardType);
        foreach (var vcType in Enum.GetValues<VotingCardType>())
        {
            var enabled = IsVotingCardTypeEnabled(vcType);
            var hasType = layoutsByType.TryGetValue(vcType, out var vcTypeLayout);
            if (enabled)
            {
                if (!hasType)
                {
                    var layout = new T { VotingCardType = vcType, AllowCustom = true };
                    votingCardLayouts.Add(layout);
                }

                continue;
            }

            if (hasType)
            {
                votingCardLayouts.Remove(vcTypeLayout!);
            }
        }
    }

    private static bool IsVotingCardTypeEnabled(VotingCardType type)
    {
        return type switch
        {
            VotingCardType.Swiss => true,
            _ => false,
        };
    }
}
