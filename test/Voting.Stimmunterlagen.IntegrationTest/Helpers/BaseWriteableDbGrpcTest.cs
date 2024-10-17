// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

/// <summary>
/// This base class uses the writeable db and the db xunit test collection to ensure only 1 test is run in parallel.
/// During initialization it resets the db and seeds the mock data (for each test).
/// </summary>
[Collection(WriteableDbTestCollection.Name)]
public abstract class BaseWriteableDbGrpcTest<TService> : BaseGrpcTest<TService, TestApplicationFactory, TestStartup>
    where TService : ClientBase<TService>
{
    protected BaseWriteableDbGrpcTest(TestApplicationFactory factory)
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

    protected Task ResetDb() => RunOnDb(DatabaseUtil.Truncate);

    protected Task SetStepState(Guid doiId, Step step, bool approved)
    {
        return ModifyDbEntities(
            (StepState state) => state.DomainOfInfluenceId == doiId && state.Step == step,
            s => s.Approved = approved);
    }

    protected Task SetContestState(Guid id, ContestState state)
    {
        return ModifyDbEntities(
            (Contest contest) => contest.Id == id,
            c => c.State = state);
    }

    protected Task SetContestBundFutureApprovedToPastSignUpDeadline()
    {
        return SetContestPrintingCenterSignUpDeadline(ContestMockData.BundFutureApprovedGuid, MockedClock.GetDate(-1));
    }

    protected Task SetContestPrintingCenterSignUpDeadline(Guid id, DateTime dateTime)
    {
        return ModifyDbEntities(
            (Contest contest) => contest.Id == id,
            c => c.PrintingCenterSignUpDeadline = dateTime);
    }

    protected Task SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline()
    {
        return SetContestGenerateVotingCardsDeadline(ContestMockData.BundFutureApprovedGuid, MockedClock.GetDate(-1));
    }

    protected Task SetContestGenerateVotingCardsDeadline(Guid id, DateTime dateTime)
    {
        return ModifyDbEntities(
            (Contest contest) => contest.Id == id,
            c => c.GenerateVotingCardsDeadline = dateTime);
    }
}
