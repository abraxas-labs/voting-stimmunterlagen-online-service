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
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VoterListImportTests;

public class DeleteVoterListImportTest : BaseWriteableDbGrpcTest<VoterListImportService.VoterListImportServiceClient>
{
    public DeleteVoterListImportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldDelete()
    {
        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });
        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggElectoralRegisterId });

        RunOnDb(db => db.VoterListImports.Any(x => x.Id == VoterListImportMockData.BundFutureApprovedGemeindeArneggGuid))
            .Should()
            .BeFalse();

        var attachment = await RunOnDb(db => db.Attachments
            .Include(x => x.DomainOfInfluenceAttachmentCounts)
            .FirstAsync(x => x.Id == AttachmentMockData.BundFutureApprovedGemeindeArneggGuid));
        attachment.TotalRequiredForVoterListsCount.Should().Be(0);
        attachment.DomainOfInfluenceAttachmentCounts!
            .Single(x => x.DomainOfInfluenceId == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid)
            .RequiredForVoterListsCount
            .Should()
            .Be(0);
    }

    [Fact]
    public async Task ShouldUpdateDomainOfInfluenceLastVoterUpdate()
    {
        var doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().BeNull();

        await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });

        doi = await RunOnDb(db => db.ContestDomainOfInfluences.FirstAsync(doi => doi.Id == DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid));
        doi.LastVoterUpdate.Should().Be(MockedClock.GetDate());
    }

    [Fact]
    public async Task ShouldThrowIfNotFound()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = "66875c4c-4bc4-4eba-9f18-a5ecefaa6c99" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundArchivedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastGenerateVotingCardsDeadline()
    {
        await SetContestBundFutureApprovedToPastGenerateVotingCardsDeadline();
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new() { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfNotOwner()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VoterListImportService.VoterListImportServiceClient service)
    {
        await service.DeleteAsync(new IdValueRequest { Id = VoterListImportMockData.BundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
