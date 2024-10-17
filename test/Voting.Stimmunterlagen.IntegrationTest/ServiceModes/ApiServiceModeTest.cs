// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.IntegrationTest.ServiceModes;

public class ApiServiceModeTest : BaseServiceModeTest<ApiServiceModeTest.PublisherAppFactory>
{
    public ApiServiceModeTest(PublisherAppFactory factory)
        : base(factory, ServiceMode.Api)
    {
    }

    public class PublisherAppFactory : ServiceModeAppFactory
    {
        public PublisherAppFactory()
            : base(ServiceMode.Api)
        {
        }
    }
}
