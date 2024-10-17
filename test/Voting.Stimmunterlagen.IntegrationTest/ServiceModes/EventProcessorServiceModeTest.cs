// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.IntegrationTest.ServiceModes;

public class EventProcessorServiceModeTest : BaseServiceModeTest<EventProcessorServiceModeTest.EventProcessorAppFactory>
{
    public EventProcessorServiceModeTest(EventProcessorAppFactory factory)
        : base(factory, ServiceMode.EventProcessor)
    {
    }

    public class EventProcessorAppFactory : ServiceModeAppFactory
    {
        public EventProcessorAppFactory()
            : base(ServiceMode.EventProcessor)
        {
        }
    }
}
