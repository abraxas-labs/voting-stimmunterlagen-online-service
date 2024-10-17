// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class SetOverriddenDomainOfInfluenceVotingCardLayoutTest :
    BaseWriteableDbGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public SetOverriddenDomainOfInfluenceVotingCardLayoutTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await GemeindeArneggElectionAdminClient.SetOverriddenLayoutAsync(NewValidRequest());

        var layout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .SingleAsync(x =>
                x.VotingCardType == Data.Models.VotingCardType.Swiss
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        layout.AllowCustom.Should().BeTrue();
        layout.TemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        layout.DomainOfInfluenceTemplateId.Should().BeNull();
        layout.OverriddenTemplateId.Should().Be(DmDocServiceMock.TemplateSwissArneggNotSeeded.Id);
        layout.EffectiveTemplateId.Should().Be(DmDocServiceMock.TemplateSwissArneggNotSeeded.Id);
    }

    [Fact]
    public async Task ShouldReset()
    {
        await GemeindeArneggElectionAdminClient.SetOverriddenLayoutAsync(new SetOverriddenDomainOfInfluenceVotingCardLayoutRequest
        {
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });

        var layout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .SingleAsync(x =>
                x.VotingCardType == Data.Models.VotingCardType.Swiss
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        layout.AllowCustom.Should().BeTrue();
        layout.TemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        layout.DomainOfInfluenceTemplateId.Should().BeNull();
        layout.OverriddenTemplateId.Should().BeNull();
        layout.EffectiveTemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetOverriddenLayoutAsync(new SetOverriddenDomainOfInfluenceVotingCardLayoutRequest
            {
                TemplateId = DmDocServiceMock.TemplateSwissArnegg.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotAllowCustom()
    {
        await ModifyDbEntities<Data.Models.DomainOfInfluenceVotingCardLayout>(
         l => l.VotingCardType == Data.Models.VotingCardType.Swiss && l.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
         l => l.AllowCustom = false);
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetOverriddenLayoutAsync(new SetOverriddenDomainOfInfluenceVotingCardLayoutRequest
            {
                TemplateId = DmDocServiceMock.TemplateSwissArneggNotSeeded.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            }),
            StatusCode.InvalidArgument,
            "custom layout is not allowed");
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetOverriddenLayoutAsync(new SetOverriddenDomainOfInfluenceVotingCardLayoutRequest
            {
                TemplateId = DmDocServiceMock.TemplateSwissArnegg.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestPastSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetOverriddenLayoutAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(
        DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.SetOverriddenLayoutAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private SetOverriddenDomainOfInfluenceVotingCardLayoutRequest NewValidRequest()
    {
        return new()
        {
            TemplateId = DmDocServiceMock.TemplateSwissArneggNotSeeded.Id,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        };
    }
}
