// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmunterlagen.Data.Models;

public class EventProcessingState : BaseEntity
{
    public static readonly Guid StaticId = Guid.Parse("b6d08709-0601-4628-b704-6aa51b1b9495");

    public EventProcessingState()
    {
        Id = StaticId;
    }

    /// <summary>
    /// Gets or sets the last processed event commit position. Set to <c>null</c> to start from the beginning.
    /// </summary>
    public ulong? LastProcessedEventCommitPosition { get; set; }

    /// <summary>
    /// Gets or sets the last processed event prepare position. Set to <c>null</c> to start from the beginning.
    /// </summary>
    public ulong? LastProcessedEventPreparePosition { get; set; }

    /// <summary>
    /// Gets or sets the last processed event number. Set to <c>null</c> to start from the beginning.
    /// </summary>
    public ulong? LastProcessedEventNumber { get; set; }

    /// <summary>
    /// Gets or sets the last event number which ever was processed.
    /// </summary>
    public ulong LatestEverProcessedEventNumber { get; set; }
}
