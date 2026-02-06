// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;
using VotingCardType = Voting.Stimmunterlagen.Proto.V1.Models.VotingCardType;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class SetDomainOfInfluenceVotingCardLayoutTest :
    BaseWriteableDbGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public SetDomainOfInfluenceVotingCardLayoutTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWorkForContestManager()
    {
        await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
        {
            AllowCustom = true,
            TemplateId = DmDocServiceMock.TemplateOthers2.Id,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            DataConfiguration = new()
            {
                IncludeReligion = true,
            },
        });

        var layout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .SingleAsync(x =>
                x.VotingCardType == Data.Models.VotingCardType.Swiss
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid));
        layout.AllowCustom.Should().BeTrue();
        layout.TemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        layout.DomainOfInfluenceTemplateId.Should().Be(DmDocServiceMock.TemplateOthers2.Id);
        layout.EffectiveTemplateId.Should().Be(DmDocServiceMock.TemplateOthers2.Id);
        layout.OverriddenTemplateId.Should().BeNull();
    }

    [Fact]
    public async Task ShouldOverrideOptionsIfStistatMunicipality()
    {
        await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
        {
            AllowCustom = true,
            TemplateId = DmDocServiceMock.TemplateOthers2.Id,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            DataConfiguration = new(),
        });

        var layout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .SingleAsync(x =>
                x.VotingCardType == Data.Models.VotingCardType.Swiss
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid));
        layout.DataConfiguration.IncludePersonId.Should().BeTrue();
        layout.DataConfiguration.IncludeDateOfBirth.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReset()
    {
        await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
        {
            AllowCustom = true,
            TemplateId = DmDocServiceMock.TemplateOthers2.Id,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            DataConfiguration = new()
            {
                IncludeIsHouseholder = true,
            },
        });
        await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
        {
            AllowCustom = true,
            VotingCardType = VotingCardType.Swiss,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            DataConfiguration = new(),
        });

        var layout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .SingleAsync(x =>
                x.VotingCardType == Data.Models.VotingCardType.Swiss
                && x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid));
        layout.AllowCustom.Should().BeTrue();
        layout.TemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        layout.DomainOfInfluenceTemplateId.Should().BeNull();
        layout.EffectiveTemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        layout.OverriddenTemplateId.Should().BeNull();
        layout.DataConfiguration.IncludeIsHouseholder.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowIfVotingCardsAlreadyGenerated()
    {
        var doiGuid = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid;

        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == doiGuid,
            doi => doi.GenerateVotingCardsTriggered = MockedClock.UtcNowDate);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
            {
                AllowCustom = true,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = doiGuid.ToString(),
                DataConfiguration = new(),
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotContestManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
            {
                AllowCustom = true,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                DataConfiguration = new(),
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfContestIsApproved()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
            {
                AllowCustom = true,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
                DataConfiguration = new(),
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfContestLocked()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetLayoutAsync(new SetDomainOfInfluenceVotingCardLayoutRequest
            {
                AllowCustom = true,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedNotApprovedGemeindeArneggId,
                DataConfiguration = new(),
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(
        DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.SetLayoutAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
