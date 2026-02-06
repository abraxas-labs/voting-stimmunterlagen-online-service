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

public class UpdateDomainOfInfluenceSettingsTest : BaseWriteableDbGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public UpdateDomainOfInfluenceSettingsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await AbraxasElectionAdminClient.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            AllowManualVoterListUpload = true,
        });

        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid));
        doi.AllowManualVoterListUpload.Should().BeTrue();

        await AbraxasElectionAdminClient.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            AllowManualVoterListUpload = false,
        });

        doi = await RunOnDb(db => db.ContestDomainOfInfluences.SingleAsync(x => x.Id == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid));
        doi.AllowManualVoterListUpload.Should().BeFalse();
    }

    [Fact]
    public Task PoliticalBusinessAdminShouldThrow()
    {
        return AssertStatus(
            async () => await StaatskanzleiStGallenElectionAdminClient.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                AllowManualVoterListUpload = true,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task AttendeeShouldThrow()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                AllowManualVoterListUpload = true,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task DomainOfInfluenceWithoutResponsibleForVotingCardsShouldThrow()
    {
        await ModifyDbEntities<ContestDomainOfInfluence>(
            x => x.Id == DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggGuid,
            x => x.ResponsibleForVotingCards = false);

        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
                AllowManualVoterListUpload = true,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceService.DomainOfInfluenceServiceClient service)
    {
        await service.UpdateSettingsAsync(new UpdateDomainOfInfluenceSettingsRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.GemeindeArneggId,
            AllowManualVoterListUpload = true,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
