// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.EventProcessors;

internal static class StepsDiffBuilder
{
    internal static (IReadOnlyCollection<StepState> ToRemove, IReadOnlyCollection<StepState> ToAdd) BuildStepsDiff(
        Guid doiId,
        IEnumerable<StepState>? existingStepStates,
        ContestRole contestRole,
        IReadOnlyCollection<PoliticalBusinessRole> politicalBusinessRoles,
        bool isSingleAttendeeContest,
        bool eVoting,
        bool externalPrintingCenter,
        bool isPoliticalAssembly,
        bool responsibleForVotingCards)
    {
        var existingStepStatesByStep = existingStepStates?.ToDictionary(x => x.Step) ?? new();
        var toRemove = new List<StepState>();
        var toAdd = new List<StepState>();
        foreach (var step in Enum.GetValues<Step>())
        {
            existingStepStatesByStep.TryGetValue(step, out var existingStepState);

            var shouldInclude = ShouldIncludeStep(
                step,
                contestRole,
                politicalBusinessRoles,
                isSingleAttendeeContest,
                eVoting,
                externalPrintingCenter,
                isPoliticalAssembly,
                responsibleForVotingCards);

            if (shouldInclude)
            {
                if (existingStepState == null)
                {
                    toAdd.Add(new StepState { DomainOfInfluenceId = doiId, Step = step });
                }

                continue;
            }

            if (existingStepState != null)
            {
                toRemove.Add(existingStepState);
            }
        }

        return (toRemove, toAdd);
    }

    private static bool ShouldIncludeStep(
        Step step,
        ContestRole contestRole,
        IReadOnlyCollection<PoliticalBusinessRole> politicalBusinessRoles,
        bool isSingleAttendeeContest,
        bool eVoting,
        bool externalPrintingCenter,
        bool isPoliticalAssembly,
        bool responsibleForVotingCards)
    {
        if (contestRole == ContestRole.None)
        {
            return false;
        }

        var isContestManager = contestRole == ContestRole.Manager;

        switch (step)
        {
            case Step.PoliticalBusinessesApproval:
                return (isContestManager || politicalBusinessRoles.Count > 0) && !isPoliticalAssembly;

            case Step.ContestApproval:
            case Step.ContestOverview:
                return isContestManager;

            case Step.EVoting:
                return isContestManager && eVoting;

            // these steps should show for the contest manager
            // except if its a contest where the contest manager is the only attendee
            case Step.LayoutVotingCardsContestManager:
            case Step.LayoutVotingCardsDomainOfInfluences:
                return isContestManager && !isSingleAttendeeContest;

            // these steps should be shown for all attendees
            case Step.LayoutVotingCardsPoliticalBusinessAttendee:
            case Step.VoterLists:
            case Step.GenerateVotingCards:
            case Step.GenerateManualVotingCards:
            case Step.VotingJournal:
                return politicalBusinessRoles.Contains(PoliticalBusinessRole.Attendee) || (responsibleForVotingCards && isPoliticalAssembly);

            // this step should be shown for all political business attendees and also managers if they do not use an external printing center.
            case Step.Attachments:
                return politicalBusinessRoles.Contains(PoliticalBusinessRole.Attendee) || (!externalPrintingCenter && (politicalBusinessRoles.Count > 0 || (responsibleForVotingCards && isPoliticalAssembly)));

            // this step should be shown for all political business managers and attendees
            case Step.PrintJobOverview:
                return politicalBusinessRoles.Count > 0 || (responsibleForVotingCards && isPoliticalAssembly);
            default:
                return false;
        }
    }
}
