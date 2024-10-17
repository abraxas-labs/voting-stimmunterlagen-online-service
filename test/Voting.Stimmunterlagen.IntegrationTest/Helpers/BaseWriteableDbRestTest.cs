// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.MockData;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public abstract class BaseWriteableDbRestTest : BaseWriteableDbTest
{
    private readonly Lazy<HttpClient> _lazyUnauthorizedClient;
    private readonly Lazy<HttpClient> _lazyAbraxasElectionAdminClient;
    private readonly Lazy<HttpClient> _lazyAbraxasPrintJobManagerClient;
    private readonly Lazy<HttpClient> _lazyGemeindeArneggClient;
    private readonly Lazy<HttpClient> _lazyStadtGossauClient;

    protected BaseWriteableDbRestTest(TestApplicationFactory factory)
        : base(factory)
    {
        _lazyUnauthorizedClient = new Lazy<HttpClient>(() => CreateHttpClient(false));
        _lazyAbraxasElectionAdminClient = new Lazy<HttpClient>(() => CreateHttpClient(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.Abraxas));
        _lazyAbraxasPrintJobManagerClient = new Lazy<HttpClient>(() => CreateHttpClient(true, roles: Roles.PrintJobManager, tenant: MockDataSeeder.SecureConnectTenantIds.Abraxas));
        _lazyGemeindeArneggClient = new Lazy<HttpClient>(() => CreateHttpClient(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.GemeindeArnegg));
        _lazyStadtGossauClient = new Lazy<HttpClient>(() => CreateHttpClient(true, roles: Roles.ElectionAdmin, tenant: MockDataSeeder.SecureConnectTenantIds.StadtGossau));
    }

    protected HttpClient UnauthorizedClient => _lazyUnauthorizedClient.Value;

    protected HttpClient AbraxasElectionAdminClient => _lazyAbraxasElectionAdminClient.Value;

    protected HttpClient AbraxasPrintJobManagerClient => _lazyAbraxasPrintJobManagerClient.Value;

    protected HttpClient GemeindeArneggClient => _lazyGemeindeArneggClient.Value;

    protected HttpClient StadtGossauClient => _lazyStadtGossauClient.Value;

    protected async Task<HttpResponseMessage> AssertStatus(Func<Task<HttpResponseMessage>> testCode, HttpStatusCode status)
    {
        var httpResponseMessage = await testCode().ConfigureAwait(continueOnCapturedContext: false);
        httpResponseMessage.StatusCode.Should().Be(status);
        return httpResponseMessage;
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
        return ModifyDbEntities(
            (Contest contest) => contest.Id == ContestMockData.BundFutureApprovedGuid,
            c => c.GenerateVotingCardsDeadline = MockedClock.GetDate(-1));
    }
}
