// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class ListDomainOfInfluenceAttachmentCountsTest : BaseReadOnlyGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public ListDomainOfInfluenceAttachmentCountsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListBundAttachmentForOwner()
    {
        var attachments = await AbraxasElectionAdminClient.ListDomainOfInfluenceAttachmentCountsAsync(new()
        { AttachmentId = AttachmentMockData.BundFutureApprovedBund1Id });
        attachments.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListBundAttachmentForAttendeeShouldReturnEmpty()
    {
        var attachments = await GemeindeArneggElectionAdminClient.ListDomainOfInfluenceAttachmentCountsAsync(new()
        { AttachmentId = AttachmentMockData.BundFutureApprovedBund1Id });
        attachments.Counts.Should().BeEmpty();
    }

    [Fact]
    public async Task ListArneggForOwnerOnLockedContest()
    {
        var attachments = await GemeindeArneggElectionAdminClient.ListDomainOfInfluenceAttachmentCountsAsync(new()
        { AttachmentId = AttachmentMockData.BundArchivedGemendeArneggId });
        attachments.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.ListDomainOfInfluenceAttachmentCountsAsync(new() { AttachmentId = AttachmentMockData.BundFutureApprovedBund1Id });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
