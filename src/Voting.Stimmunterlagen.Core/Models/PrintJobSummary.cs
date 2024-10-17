// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Core.Models;

public class PrintJobSummary
{
    public PrintJobSummary(
        PrintJob printJob,
        int attachmentsDefinedCount,
        int attachmentsDeliveredCount,
        bool hasCommunalPoliticalBusinesses,
        string templateName,
        Step lastConfirmedStep)
    {
        DomainOfInfluence = printJob.DomainOfInfluence;
        State = printJob.State;
        ProcessStartedOn = printJob.ProcessStartedOn;
        ProcessEndedOn = printJob.ProcessEndedOn;
        DoneOn = printJob.DoneOn;
        AttachmentsDefinedCount = attachmentsDefinedCount;
        AttachmentsDeliveredCount = attachmentsDeliveredCount;
        HasCommunalPoliticalBusinesses = hasCommunalPoliticalBusinesses;
        TemplateName = templateName;
        LastConfirmedStep = lastConfirmedStep;
    }

    public ContestDomainOfInfluence? DomainOfInfluence { get; }

    public PrintJobState State { get; }

    public DateTime? ProcessStartedOn { get; }

    public DateTime? ProcessEndedOn { get; }

    public DateTime? DoneOn { get; }

    public int AttachmentsDefinedCount { get; }

    public int AttachmentsDeliveredCount { get; }

    public bool HasCommunalPoliticalBusinesses { get; }

    public string TemplateName { get; }

    public Step LastConfirmedStep { get; }
}
