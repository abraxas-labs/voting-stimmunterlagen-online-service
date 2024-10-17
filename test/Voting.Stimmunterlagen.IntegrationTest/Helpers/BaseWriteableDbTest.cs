// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

/// <summary>
/// This base class uses the writeable db and the db xunit test collection to ensure only 1 test is run in parallel.
/// During initialization it resets the db and seeds the mock data (for each test).
/// </summary>
[Collection(WriteableDbTestCollection.Name)]
public abstract class BaseWriteableDbTest : BaseDbTest<TestApplicationFactory, TestStartup>
{
    protected BaseWriteableDbTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await ResetDb();
        await MockDataSeeder.Seed(RunScoped);
    }

    protected Task ModifyDbEntities<TEntity>(Expression<Func<TEntity, bool>> predicate, Action<TEntity> modifier)
        where TEntity : class
    {
        return RunOnDb(async db =>
        {
            var set = db.Set<TEntity>();
            var entities = await set.AsTracking().Where(predicate).ToListAsync();

            foreach (var entity in entities)
            {
                modifier(entity);
            }

            await db.SaveChangesAsync();
        });
    }
}
