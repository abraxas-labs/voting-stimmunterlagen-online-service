// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class DeleteAttachmentTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public DeleteAttachmentTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldDelete()
    {
        var id = AttachmentMockData.BundFutureApprovedBund1Guid;
        await AbraxasElectionAdminClient.DeleteAsync(new() { Id = AttachmentMockData.BundFutureApprovedBund1Id });

        var attachment = await RunOnDb(db => db.Attachments.SingleOrDefaultAsync(x => x.Id == id));
        attachment.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowIfNotDoiManager()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new() { Id = AttachmentMockData.BundFutureApprovedBund1Id }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfContestLocked()
    {
        await AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.DeleteAsync(new() { Id = AttachmentMockData.BundArchivedGemendeArneggId }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowIfPastContestSignUpDeadline()
    {
        await SetContestBundFutureApprovedToPastSignUpDeadline();
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.DeleteAsync(new() { Id = AttachmentMockData.BundFutureApprovedBund1Id }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.DeleteAsync(new() { Id = AttachmentMockData.BundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
