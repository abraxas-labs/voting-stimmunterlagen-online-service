// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.AttachmentTests;

public class ListAttachmentCategorySummariesTest : BaseWriteableDbGrpcTest<AttachmentService.AttachmentServiceClient>
{
    public ListAttachmentCategorySummariesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForDomainOfInfluenceBund()
    {
        var summaries = await AbraxasElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceGemeindeArnegg()
    {
        var summaries = await GemeindeArneggElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceGemeindeArneggWithHouseholdersOnlyAttachments()
    {
        var summaries = await GemeindeArneggElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });

        summaries.Summaries[0].TotalRequiredForVoterListsCount.Should().Be(9);

        await ModifyDbEntities<Data.Models.Attachment>(
            a => a.Id == AttachmentMockData.BundFutureApprovedBund1Guid,
            a => a.SendOnlyToHouseholder = true);

        await ModifyDbEntities<Data.Models.VoterList>(vl => vl.Id == VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid, vl => vl.NumberOfHouseholders = 0);

        // The count should not change if not all attachments of a category are sent to householders.
        summaries = await GemeindeArneggElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });

        summaries.Summaries[0].TotalRequiredForVoterListsCount.Should().Be(9);

        await ModifyDbEntities<Data.Models.Attachment>(
            a => a.Id == AttachmentMockData.BundFutureApprovedBund2Guid,
            a => a.SendOnlyToHouseholder = true);

        summaries = await GemeindeArneggElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });

        summaries.Summaries[0].TotalRequiredForVoterListsCount.Should().Be(6);
    }

    [Fact]
    public async Task ListForDomainOfInfluenceUzwil()
    {
        var summaries = await StadtUzwilElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtUzwilId });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceArneggOnLockedContest()
    {
        var summaries = await GemeindeArneggElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundArchivedGemeindeArneggId });
        summaries.Summaries.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListForDomainOfInfluenceAsPrintJobManager()
    {
        var summaries = await GemeindeArneggPrintJobManagerClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForDomainOfInfluenceAsElectionAdmin()
    {
        var summaries = await AbraxasElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyForDomainOfInfluenceIfElectionAdminAndNotContestManager()
    {
        var result = await StadtGossauElectionAdminClient.ListCategorySummariesAsync(new()
        { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId });

        result.Summaries.Should().HaveCount(0);
    }

    [Fact]
    public async Task ListForFilterAttachmentName()
    {
        var summaries = await AbraxasElectionAdminClient.ListCategorySummariesAsync(new()
        {
            Filter = new ListAttachmentCategorySummariesFilterRequest
            {
                ContestId = ContestMockData.BundFutureApprovedId,
                Query = "ossau",
            },
        });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForFilterAuthorityName()
    {
        var summaries = await AbraxasPrintJobManagerClient.ListCategorySummariesAsync(new()
        {
            Filter = new ListAttachmentCategorySummariesFilterRequest
            {
                ContestId = ContestMockData.BundFutureApprovedId,
                Query = "kAnzlei",
            },
        });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ListForFilterOnlyDelivered()
    {
        var summaries = await AbraxasPrintJobManagerClient.ListCategorySummariesAsync(new()
        {
            Filter = new ListAttachmentCategorySummariesFilterRequest
            {
                ContestId = ContestMockData.BundFutureApprovedId,
                State = AttachmentState.Delivered,
            },
        });
        summaries.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyForFilterIfElectionAdminAndNotContestManager()
    {
        var result = await StadtGossauElectionAdminClient.ListCategorySummariesAsync(new()
        {
            Filter = new ListAttachmentCategorySummariesFilterRequest
            {
                ContestId = ContestMockData.BundFutureApprovedId,
            },
        });

        result.Summaries.Should().HaveCount(0);
    }

    [Fact]
    public async Task ShouldReturnEmptyForPrintJobManagementIfInvalidDoi()
    {
        var result = await AbraxasPrintJobManagerClient.ListCategorySummariesAsync(new()
        {
            DomainOfInfluenceId = "cecb9be3-462f-4412-a023-f76a583ca0d2",
        });

        result.Summaries.Should().HaveCount(0);
    }

    protected override async Task AuthorizationTestCall(AttachmentService.AttachmentServiceClient service)
    {
        await service.ListCategorySummariesAsync(new() { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
