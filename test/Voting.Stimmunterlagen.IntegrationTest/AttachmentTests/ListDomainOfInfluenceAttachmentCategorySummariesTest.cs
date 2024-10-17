// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Snapper;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class ListDomainOfInfluenceAttachmentCategorySummariesTest : BaseReadOnlyGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public ListDomainOfInfluenceAttachmentCategorySummariesTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForDomainOfInfluenceBund()
    {
        var attachments = await AbraxasElectionAdminClient.ListDomainOfInfluenceAttachmentCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        attachments.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceGemeindeArnegg()
    {
        var attachments = await GemeindeArneggElectionAdminClient.ListDomainOfInfluenceAttachmentCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
        attachments.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceUzwil()
    {
        var attachments = await StadtUzwilElectionAdminClient.ListDomainOfInfluenceAttachmentCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilId });
        attachments.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceArneggOnLockedContest()
    {
        var attachments = await GemeindeArneggElectionAdminClient.ListDomainOfInfluenceAttachmentCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId });
        attachments.ShouldMatchSnapshot();
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.ListDomainOfInfluenceAttachmentCategorySummariesAsync(new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
