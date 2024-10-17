// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Abraxas.Voting.Basis.Events.V1;
using Abraxas.Voting.Basis.Shared.V1;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Eventing.Testing.Mocks;
using Voting.Lib.Iam.Testing.AuthenticationScheme;
using Voting.Lib.Testing;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using DomainOfInfluenceType = Abraxas.Voting.Basis.Shared.V1.DomainOfInfluenceType;

namespace Voting.Stimmunterlagen.IntegrationTest.ServiceModes;

[Collection(WriteableDbTestCollection.Name)]
public abstract class BaseServiceModeTest<TFactory> : BaseTest<TFactory, ServiceModeAppStartup>
    where TFactory : BaseTestApplicationFactory<ServiceModeAppStartup>
{
    private readonly ServiceMode _serviceMode;

    protected BaseServiceModeTest(TFactory factory, ServiceMode serviceMode)
        : base(factory)
    {
        _serviceMode = serviceMode;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await RunScoped((DataContext db) => DatabaseUtil.Truncate(db));
        await MockDataSeeder.Seed(RunScoped);
    }

    [Fact]
    public async Task WriteEndpointShouldWorkIfApi()
    {
        if (!_serviceMode.HasFlag(ServiceMode.Api))
        {
            return;
        }

        using var channel = CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg);
        var client = new AttachmentService.AttachmentServiceClient(channel);
        var resp = await client.CreateAsync(new CreateAttachmentRequest
        {
            Name = "Publikation Gossau",
            Format = AttachmentFormat.A5,
            Color = "Blue",
            Supplier = "Lieferant",
            DeliveryPlannedOn = MockedClock.GetTimestampDate(10),
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            PoliticalBusinessIds =
            {
                VoteMockData.BundFutureApprovedGemeindeArnegg1Id,
                ProportionalElectionMockData.BundFutureApprovedGemeindeArnegg1Id,
            },
            OrderedCount = 2000,
            RequiredCount = 2000,
            Category = AttachmentCategory.BrochureMu,
        });

        var attachment = await RunScoped((DataContext db) => db.Attachments.SingleAsync(x => x.Id == Guid.Parse(resp.Id)));
        attachment.Name.Should().Be("Publikation Gossau");
    }

    [Fact]
    public async Task WriteEndpointShouldThrowIfNotApi()
    {
        if (_serviceMode.HasFlag(ServiceMode.Api))
        {
            return;
        }

        using var channel = CreateGrpcChannel(tenant: SecureConnectTestDefaults.MockedTenantGossau.Id, roles: Roles.ElectionAdmin);
        var client = new AttachmentService.AttachmentServiceClient(channel);
        var ex = await Assert.ThrowsAnyAsync<RpcException>(async () => await client.CreateAsync(new()));
        ex.StatusCode.Should().Be(StatusCode.Unimplemented);
    }

    [Fact]
    public async Task ReadEndpointShouldWorkIfApi()
    {
        if (!_serviceMode.HasFlag(ServiceMode.Api))
        {
            return;
        }

        using var channel = CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StaatskanzleiStGallen);
        var client = new ContestService.ContestServiceClient(channel);
        var contest = await client.GetAsync(new() { Id = ContestMockData.BundFutureId });
        contest.Id.Should().Be(ContestMockData.BundFutureId);
    }

    [Fact]
    public async Task ReadEndpointShouldThrowIfNotApi()
    {
        if (_serviceMode.HasFlag(ServiceMode.Api))
        {
            return;
        }

        using var channel = CreateGrpcChannel(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StaatskanzleiStGallen);
        var client = new ContestService.ContestServiceClient(channel);
        var ex = await Assert.ThrowsAsync<RpcException>(async () => await client.GetAsync(new() { Id = ContestMockData.BundFutureId }));
        ex.StatusCode.Should().Be(StatusCode.Unimplemented);
    }

    [Fact]
    public async Task EventProcessingShouldWorkIfEventProcessor()
    {
        if (!_serviceMode.HasFlag(ServiceMode.EventProcessor))
        {
            return;
        }

        var testPublisher = GetService<TestEventPublisher>();

        var id = Guid.Parse("7ef5e239-be6f-4d4c-89c9-b3a39cdc41ff");
        await testPublisher.Publish(new DomainOfInfluenceCreated
        {
            DomainOfInfluence = new()
            {
                Bfs = "123",
                Code = "123",
                Id = id.ToString(),
                Name = "test",
                Canton = DomainOfInfluenceCanton.Sg,
                Type = DomainOfInfluenceType.An,
                AuthorityName = "SG",
                ShortName = "SG",
                SortNumber = 1,
                SecureConnectId = "cf170e3b-9f65-403e-9da8-e98cabd362a3",
                ResponsibleForVotingCards = true,
            },
            EventInfo = new() { Timestamp = MockedClock.UtcNowTimestamp },
        });

        var doi = await GetService<DataContext>().DomainOfInfluences.SingleAsync(x => x.Id == id);
        doi.Bfs.Should().Be("123");
    }

    [Fact(Skip = "Metric endpoint test is not working properly with dedicated prometheus metric server port (ref: VOTING-4006)")]
    public async Task MetricsEndpointShouldWork()
    {
        var client = CreateHttpClient(false);
        var response = await client.GetPrometheusMetricsAsync();
        response
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task HealthEndpointShouldWork()
    {
        var client = CreateHttpClient(false);

        var result = await client.GetStringAsync("/healthz");
        result.Should().Be("Healthy");
    }
}
