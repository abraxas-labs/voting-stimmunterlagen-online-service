// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.Data.Models;

public enum PrintJobState
{
    Unspecified,
    Empty,
    SubmissionOngoing,
    ReadyForProcess,
    ProcessStarted,
    ProcessEnded,
    Done,
}
