// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class TestReadOnlyApplicationFactory : TestApplicationFactory<TestReadOnlyDbStartup>, IAsyncLifetime
{
    private static readonly AsyncLock MockDataSeederLock = new();
    private static volatile bool _mockDataLoaded;

    public async Task InitializeAsync()
    {
        // this method is executed once per test class
        // but we want too seed data only once.
        if (_mockDataLoaded)
        {
            return;
        }

        using var locker = await MockDataSeederLock.AcquireAsync();
        if (_mockDataLoaded)
        {
            return;
        }

        await RunScoped(sp => DatabaseUtil.Truncate(sp.GetRequiredService<DataContext>()));
        await MockDataSeeder.Seed(RunScoped);

        _mockDataLoaded = true;
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
