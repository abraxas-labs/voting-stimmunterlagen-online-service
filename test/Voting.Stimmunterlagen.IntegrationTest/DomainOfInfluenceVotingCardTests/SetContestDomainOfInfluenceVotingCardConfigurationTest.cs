// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using DomainOfInfluenceVotingCardConfiguration = Voting.Stimmunterlagen.Data.Models.DomainOfInfluenceVotingCardConfiguration;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardTests;

public class SetContestDomainOfInfluenceVotingCardConfigurationTest
    : BaseWriteableDbGrpcTest<DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient>
{
    public SetContestDomainOfInfluenceVotingCardConfigurationTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await GemeindeArneggElectionAdminClient.SetConfigurationAsync(NewValidRequest());

        var entity = await FindDbEntity<DomainOfInfluenceVotingCardConfiguration>(x =>
            x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid);
        entity.MatchSnapshot();
    }

    [Fact]
    public Task ShouldThrowOtherTenant()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetConfigurationAsync(new SetDomainOfInfluenceVotingCardConfigurationRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
                SampleCount = 10,
                VotingCardGroups = { VotingCardGroup.ShippingRegion, VotingCardGroup.Language },
                VotingCardSorts = { VotingCardSort.Name },
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfContestLocked()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetConfigurationAsync(new SetDomainOfInfluenceVotingCardConfigurationRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId,
                SampleCount = 10,
                VotingCardGroups = { VotingCardGroup.ShippingRegion, VotingCardGroup.Language },
                VotingCardSorts = { VotingCardSort.Name },
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetConfigurationAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient service)
    {
        await service.SetConfigurationAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private SetDomainOfInfluenceVotingCardConfigurationRequest NewValidRequest()
    {
        return new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            SampleCount = 10,
            VotingCardGroups = { VotingCardGroup.ShippingRegion, VotingCardGroup.Language },
            VotingCardSorts = { VotingCardSort.Name },
        };
    }
}
