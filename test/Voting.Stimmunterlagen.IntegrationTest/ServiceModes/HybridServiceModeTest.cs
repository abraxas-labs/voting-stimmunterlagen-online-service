// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.IntegrationTest.ServiceModes;

public class HybridServiceModeTest : BaseServiceModeTest<HybridServiceModeTest.HybridAppFactory>
{
    public HybridServiceModeTest(HybridAppFactory factory)
        : base(factory, ServiceMode.Hybrid)
    {
    }

    public class HybridAppFactory : ServiceModeAppFactory
    {
        public HybridAppFactory()
            : base(ServiceMode.Hybrid)
        {
        }
    }
}
