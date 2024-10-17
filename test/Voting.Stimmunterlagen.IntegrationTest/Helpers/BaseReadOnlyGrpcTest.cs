// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

/// <summary>
/// A base test class which runs in parallel. The database is initialized with mock data only once.
/// </summary>
public abstract class BaseReadOnlyGrpcTest<TService> : BaseGrpcTest<TService, TestReadOnlyApplicationFactory, TestReadOnlyDbStartup>
    where TService : ClientBase<TService>
{
    public BaseReadOnlyGrpcTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }
}
