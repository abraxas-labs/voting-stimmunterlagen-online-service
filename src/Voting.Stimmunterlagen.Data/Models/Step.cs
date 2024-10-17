// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

// if this is changed the db and the proto may need updates too
// after a change is deployed a call to abraxas.voting.stimmunterlagen.v1.StepService.Sync is required
public enum Step
{
    Unspecified,
    PoliticalBusinessesApproval,
    LayoutVotingCardsContestManager,
    LayoutVotingCardsDomainOfInfluences,
    LayoutVotingCardsPoliticalBusinessAttendee,
    ContestApproval,
    Attachments,
    VoterLists,
    GenerateVotingCards,
    EVoting,
    PrintJobOverview,
    GenerateManualVotingCards,
    ContestOverview,
    VotingJournal,
}
