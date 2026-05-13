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
    protected void SyncVotingCardLayouts(ICollection<T> votingCardLayouts, DomainOfInfluenceVotingCardPrintData? printData)
    {
        var layoutsByType = votingCardLayouts.ToDictionary(x => x.VotingCardType);
        foreach (var vcType in Enum.GetValues<VotingCardType>())
        {
            var enabled = IsVotingCardTypeEnabled(vcType);
            var hasType = layoutsByType.TryGetValue(vcType, out var vcTypeLayout);
            if (enabled && hasType)
            {
                UpdateLayoutPrintData(votingCardLayouts, printData, vcType);
            }

            if (enabled && !hasType)
            {
                AddLayout(votingCardLayouts, printData, vcType);
            }

            if (!enabled && hasType)
            {
                votingCardLayouts.Remove(vcTypeLayout!);
            }
        }
    }

    private static void UpdateLayoutPrintData(ICollection<T> votingCardLayouts, DomainOfInfluenceVotingCardPrintData? printData, VotingCardType vcType)
    {
        foreach (var layout in votingCardLayouts.Where(x => x.VotingCardType == vcType))
        {
            layout.PrintData = printData is null ? null : new(printData);
        }
    }

    private static void AddLayout(ICollection<T> votingCardLayouts, DomainOfInfluenceVotingCardPrintData? printData, VotingCardType vcType)
    {
        var layout = new T { VotingCardType = vcType, AllowCustom = true, PrintData = printData is null ? null : new(printData) };
        votingCardLayouts.Add(layout);
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
