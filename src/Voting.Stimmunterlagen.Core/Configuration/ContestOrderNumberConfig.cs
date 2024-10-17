// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class ContestOrderNumberConfig
{
    public int Min { get; set; } = 900000;

    public int Max { get; set; } = 999999;

    /// <summary>
    /// Gets or sets the overlap free timespan within, a contest order number should not be repeated
    /// (conflict with external system).
    /// </summary>
    public TimeSpan OverlapFreeTimespan { get; set; } = TimeSpan.FromDays(365 * 5); // According to swiss post, 1 year. We use a larger timespan to be on the safe side.
}
