// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

/// <summary>
/// A base test class which runs in parallel. The database is initialized with mock data only once.
/// </summary>
public abstract class BaseReadOnlyDbTest : BaseDbTest<TestReadOnlyApplicationFactory, TestReadOnlyDbStartup>
{
    protected BaseReadOnlyDbTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }
}
