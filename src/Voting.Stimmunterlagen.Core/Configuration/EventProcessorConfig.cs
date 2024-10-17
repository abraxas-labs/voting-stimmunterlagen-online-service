// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Eventing.Configuration;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class EventProcessorConfig
{
    public EventStoreConfig EventStore { get; set; } = new();

    public ContestOrderNumberConfig ContestOrderNumber { get; set; } = new();
}
