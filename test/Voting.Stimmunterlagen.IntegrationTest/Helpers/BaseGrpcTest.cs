// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.IntegrationTest.MockData;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public abstract class BaseGrpcTest<TService, TFactory, TStartup> : GrpcAuthorizationBaseTest<TFactory, TStartup>
    where TService : ClientBase<TService>
    where TStartup : class
    where TFactory : BaseTestApplicationFactory<TStartup>
{
    private readonly Lazy<TService> _lazyUnauthorizedClient;
    private readonly Lazy<TService> _lazyAbraxasElectionAdminClient;
    private readonly Lazy<TService> _lazyStadtGossauElectionAdminClient;
    private readonly Lazy<TService> _lazyStadtUzwilElectionAdminClient;
    private readonly Lazy<TService> _lazyGemeindeArneggElectionAdminClient;
    private readonly Lazy<TService> _lazyStaatskanzleiStGallenElectionAdminClient;
    private readonly Lazy<TService> _lazyAbraxasPrintJobManagerClient;
    private readonly Lazy<TService> _lazyStadtGossauPrintJobManagerClient;
    private readonly Lazy<TService> _lazyGemeindeArneggPrintJobManagerClient;
    private readonly Lazy<TService> _lazyUnknownClient;

    protected BaseGrpcTest(TFactory factory)
        : base(factory)
    {
        _lazyUnauthorizedClient = new(() => CreateGrpcService(CreateGrpcChannel(false)));
        _lazyUnknownClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.Unknown)));
        _lazyAbraxasElectionAdminClient =
            new(() => CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.Abraxas)));
        _lazyGemeindeArneggElectionAdminClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg)));
        _lazyStadtGossauElectionAdminClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StadtGossau)));
        _lazyStadtUzwilElectionAdminClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StadtUzwil)));
        _lazyStaatskanzleiStGallenElectionAdminClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StaatskanzleiStGallen)));
        _lazyStadtGossauPrintJobManagerClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.PrintJobManager, tenant: MockDataSeeder.SecureConnectTenantIds.StadtGossau)));
        _lazyAbraxasPrintJobManagerClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.PrintJobManager, tenant: MockDataSeeder.SecureConnectTenantIds.Abraxas)));
        _lazyGemeindeArneggPrintJobManagerClient = new(() =>
            CreateGrpcService(CreateGrpcChannel(true, roles: Roles.PrintJobManager, tenant: MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg)));
    }

    protected TService UnauthorizedClient => _lazyUnauthorizedClient.Value;

    protected TService UnknownClient => _lazyUnknownClient.Value;

    protected TService AbraxasElectionAdminClient => _lazyAbraxasElectionAdminClient.Value;

    protected TService GemeindeArneggElectionAdminClient => _lazyGemeindeArneggElectionAdminClient.Value;

    protected TService StaatskanzleiStGallenElectionAdminClient => _lazyStaatskanzleiStGallenElectionAdminClient.Value;

    protected TService StadtGossauElectionAdminClient => _lazyStadtGossauElectionAdminClient.Value;

    protected TService StadtUzwilElectionAdminClient => _lazyStadtUzwilElectionAdminClient.Value;

    protected TService AbraxasPrintJobManagerClient => _lazyAbraxasPrintJobManagerClient.Value;

    protected TService StadtGossauPrintJobManagerClient => _lazyStadtGossauPrintJobManagerClient.Value;

    protected TService GemeindeArneggPrintJobManagerClient => _lazyGemeindeArneggPrintJobManagerClient.Value;

    protected void RunOnDb(Action<DataContext> action)
        => RunScoped(action);

    protected TResult RunOnDb<TResult>(Func<DataContext, TResult> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<DataContext, Task> action)
        => RunScoped(action);

    protected Task<TResult> RunOnDb<TResult>(Func<DataContext, Task<TResult>> action)
        => RunScoped(action);

    protected Task<TEntity> FindDbEntity<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
    {
        return RunOnDb(db => db.Set<TEntity>().Where(predicate).SingleAsync());
    }

    protected Task<List<TEntity>> FindDbEntities<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : class
    {
        return RunOnDb(db => db.Set<TEntity>().Where(predicate).ToListAsync());
    }

    protected TService CreateGrpcService(GrpcChannel channel)
        => (TService)Activator.CreateInstance(typeof(TService), channel)!;

    protected override Task AuthorizationTestCall(GrpcChannel channel)
        => AuthorizationTestCall(CreateGrpcService(channel));

    protected abstract Task AuthorizationTestCall(TService service);

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
