// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;
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
