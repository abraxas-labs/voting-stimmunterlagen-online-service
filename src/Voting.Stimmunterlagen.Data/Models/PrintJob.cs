// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class PrintJob : BaseEntity, IHasContestDomainOfInfluence
{
    public ContestDomainOfInfluence? DomainOfInfluence { get; set; }

    public Guid DomainOfInfluenceId { get; set; }

    public PrintJobState State { get; set; }

    public DateTime? ProcessStartedOn { get; set; }

    public DateTime? ProcessEndedOn { get; set; }

    public DateTime? DoneOn { get; set; }

    public int VotingCardsPrintedAndPackedCount { get; set; }

    public double VotingCardsShipmentWeight { get; set; }

    public string DoneComment { get; set; } = string.Empty;

    /// <summary>
    /// When all attachments of the doi were once delivered this returns true. When an attachment unsets the delivery state, this still remains true.
    /// This is used, since further print job state changes to empty or submission started are currently as soon as all attachments were delivered.
    /// Implemented according VOTING-1204.
    /// </summary>
    /// <returns>A boolean indicating whether all attachments of the doi were once delivered.</returns>
    public bool AllAttachmentsOnceDelivered()
    {
        return State >= PrintJobState.ReadyForProcess;
    }

    /// <summary>
    /// Returns true if a print job manager has started the process explicitly. The print job manager has to explicitly revert the process start, otherwise
    /// print job state changes depending on attachment state changes are ignored.
    /// Implemented according VOTING-1204.
    /// </summary>
    /// <returns>A boolean indicating whether a print job manager has started the process explicitly.</returns>
    public bool ProcessStarted()
    {
        return State >= PrintJobState.ProcessStarted;
    }
}
