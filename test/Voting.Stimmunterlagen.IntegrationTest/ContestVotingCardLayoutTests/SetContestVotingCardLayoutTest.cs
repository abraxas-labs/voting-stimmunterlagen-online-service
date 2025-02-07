// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
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

namespace Voting.Stimmunterlagen.IntegrationTest.ContestVotingCardLayoutTests;

public class SetContestVotingCardLayoutTest : BaseWriteableDbGrpcTest<ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient>
{
    public SetContestVotingCardLayoutTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await AbraxasElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
        {
            AllowCustom = true,
            ContestId = ContestMockData.BundFutureId,
            TemplateId = DmDocServiceMock.TemplateOthers2.Id,
            VotingCardType = VotingCardType.Swiss,
        });

        var contestLayout = await RunOnDb(db => db.ContestVotingCardLayouts
            .SingleAsync(x => x.VotingCardType == Data.Models.VotingCardType.Swiss && x.ContestId == ContestMockData.BundFutureGuid));
        contestLayout.AllowCustom.Should().BeTrue();
        contestLayout.TemplateId.Should().Be(DmDocServiceMock.TemplateOthers2.Id);

        var doiLayouts = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts
            .Where(x => x.VotingCardType == Data.Models.VotingCardType.Swiss && x.DomainOfInfluence!.ContestId == ContestMockData.BundFutureGuid)
            .ToListAsync());
        doiLayouts.All(x => x.AllowCustom).Should().BeTrue();
        doiLayouts.All(x => x.TemplateId == DmDocServiceMock.TemplateOthers2.Id).Should().BeTrue();
        doiLayouts.All(x => x.DomainOfInfluenceTemplateId == null).Should().BeTrue();
        doiLayouts.All(x => x.OverriddenTemplateId == null).Should().BeTrue();
        doiLayouts.All(x => x.EffectiveTemplateId == DmDocServiceMock.TemplateOthers2.Id).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldIgnoreDoisWithVotingCardsAlreadyGenerated()
    {
        var unaffectedDoiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid;
        var affectedDoiGuid = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauGuid;

        await ModifyDbEntities<Contest>(
            c => c.Id == ContestMockData.BundFutureApprovedGuid,
            c => c.Approved = null);

        await ModifyDbEntities<ContestDomainOfInfluence>(
            doi => doi.Id == unaffectedDoiGuid,
            doi => doi.GenerateVotingCardsTriggered = MockedClock.UtcNowDate);

        await ModifyDbEntities<DomainOfInfluenceVotingCardLayout>(
            l => l.DomainOfInfluenceId == unaffectedDoiGuid || l.DomainOfInfluenceId == affectedDoiGuid,
            l =>
            {
                l.TemplateId = DmDocServiceMock.TemplateSwiss.Id;
                l.DomainOfInfluenceTemplateId = DmDocServiceMock.TemplateSwiss.Id;
                l.OverriddenTemplateId = DmDocServiceMock.TemplateOthers.Id;
                l.AllowCustom = true;
            });

        await AbraxasElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
        {
            AllowCustom = false,
            ContestId = ContestMockData.BundFutureApprovedId,
            TemplateId = DmDocServiceMock.TemplateOthers2.Id,
            VotingCardType = VotingCardType.Swiss,
        });

        var affectedLayout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts.SingleAsync(l => l.DomainOfInfluenceId == affectedDoiGuid));
        var unaffectedLayout = await RunOnDb(db => db.DomainOfInfluenceVotingCardLayouts.SingleAsync(l => l.DomainOfInfluenceId == unaffectedDoiGuid));

        unaffectedLayout.TemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        unaffectedLayout.DomainOfInfluenceTemplateId.Should().Be(DmDocServiceMock.TemplateSwiss.Id);
        unaffectedLayout.OverriddenTemplateId.Should().Be(DmDocServiceMock.TemplateOthers.Id);
        unaffectedLayout.AllowCustom.Should().BeTrue();

        affectedLayout.TemplateId.Should().Be(DmDocServiceMock.TemplateOthers2.Id);
        affectedLayout.DomainOfInfluenceTemplateId.Should().Be(null);
        affectedLayout.OverriddenTemplateId.Should().Be(null);
        affectedLayout.AllowCustom.Should().BeFalse();
    }

    [Fact]
    public Task ShouldThrowIfApproved()
    {
        return AssertStatus(
            async () => await AbraxasElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
            {
                AllowCustom = true,
                ContestId = ContestMockData.BundFutureApprovedId,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfNotContestManager()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
            {
                AllowCustom = true,
                ContestId = ContestMockData.BundFutureId,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfEVotingOnNonEVotingContest()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
            {
                AllowCustom = true,
                ContestId = ContestMockData.SchulgemeindeAndwilArneggFutureId,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.EVoting,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task ShouldThrowIfContestLocked()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.SetLayoutAsync(new SetContestVotingCardLayoutRequest
            {
                AllowCustom = true,
                ContestId = ContestMockData.BundArchivedId,
                TemplateId = DmDocServiceMock.TemplateOthers2.Id,
                VotingCardType = VotingCardType.Swiss,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient service)
    {
        await service.SetLayoutAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
