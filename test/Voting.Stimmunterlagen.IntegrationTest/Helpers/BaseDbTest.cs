// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Eventing.Testing.Mocks;
using Voting.Lib.Testing;
using Voting.Stimmunterlagen.Data;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class BaseDbTest<TFactory, TStartup> : BaseTest<TFactory, TStartup>
    where TStartup : class
    where TFactory : BaseTestApplicationFactory<TStartup>
{
    protected BaseDbTest(TFactory factory)
        : base(factory)
    {
        TestEventPublisher = GetService<TestEventPublisher>();
    }

    protected TestEventPublisher TestEventPublisher { get; }

    protected void RunOnDb(Action<DataContext> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<DataContext, Task> action, string? language = null)
    {
        return RunScoped<DataContext>(db =>
        {
            db.Language = language;
            return action(db);
        });
    }

    protected Task<TResult> RunOnDb<TResult>(Func<DataContext, Task<TResult>> action, string? language = null)
    {
        return RunScoped<DataContext, TResult>(db =>
        {
            db.Language = language;
            return action(db);
        });
    }

    protected Task<TEntity> GetDbEntity<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
        => RunOnDb(db => db.Set<TEntity>().FirstAsync(predicate));

    protected Task ResetDb() => RunOnDb(DatabaseUtil.Truncate);

    protected async Task<T> AssertException<T>(Func<Task> testCode, string message)
        where T : Exception
    {
        var ex = await Assert.ThrowsAsync<T>(testCode);
        ex.Message.Should().Contain(message);
        return ex;
    }

    protected Task SetEventProcessingState(ulong? lastProcessedEventNumber, ulong latestEverProcessedEventNumber)
        => RunOnDb(db => EventProcessingStateUtil.SetEventProcessingState(db, lastProcessedEventNumber, latestEverProcessedEventNumber));
}
