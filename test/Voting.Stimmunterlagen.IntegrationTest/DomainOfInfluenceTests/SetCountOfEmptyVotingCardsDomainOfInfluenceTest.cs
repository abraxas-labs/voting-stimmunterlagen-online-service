// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceTests;

public class SetCountOfEmptyVotingCardsDomainOfInfluenceTest : BaseWriteableDbGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public SetCountOfEmptyVotingCardsDomainOfInfluenceTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await GemeindeArneggElectionAdminClient.SetCountOfEmptyVotingCardsAsync(new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            CountOfEmptyVotingCards = 10,
        });

        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.CountOfEmptyVotingCards.Should().Be(10);
        doi.LastCountOfEmptyVotingCardsUpdate.Should().NotBeNull();
    }

    [Fact]
    public Task ShouldThrowIfForeignTenant()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetCountOfEmptyVotingCardsAsync(new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                CountOfEmptyVotingCards = 1,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetCountOfEmptyVotingCardsAsync(
                new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
                {
                    DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId,
                    CountOfEmptyVotingCards = 1,
                }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetCountOfEmptyVotingCardsAsync(
                new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
                {
                    DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
                    CountOfEmptyVotingCards = 1,
                }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithoutEnabledEmptyVotingCards()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
            x => x.HasEmptyVotingCards = false);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetCountOfEmptyVotingCardsAsync(new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                CountOfEmptyVotingCards = 2,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceService.DomainOfInfluenceServiceClient service)
    {
        await service.SetCountOfEmptyVotingCardsAsync(new SetCountOfEmptyVotingCardsDomainOfInfluenceRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.GemeindeArneggId,
            CountOfEmptyVotingCards = 2,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
