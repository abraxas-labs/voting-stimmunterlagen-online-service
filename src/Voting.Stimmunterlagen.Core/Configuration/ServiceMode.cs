// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmunterlagen.Core.Configuration;

[Flags]
public enum ServiceMode
{
    /// <summary>
    /// The app acts as an API service (reader and writer) but does not update the event-related database read model.
    /// </summary>
    Api = 1 << 0,

    /// <summary>
    /// The app acts as event processor but does not expose endpoints (except for monitoring endpoints such as health and metrics).
    /// </summary>
    EventProcessor = 1 << 1,

    /// <summary>
    /// The app acts as <see cref="Api"/> and <see cref="EventProcessor"/>.
    /// </summary>
    Hybrid = Api | EventProcessor,
}
