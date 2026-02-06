// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.PoliticalBusinessesTests;

public class ListAttachmentAccessiblePoliticalBusinessesTest : BaseReadOnlyGrpcTest<PoliticalBusinessService.PoliticalBusinessServiceClient>
{
    public ListAttachmentAccessiblePoliticalBusinessesTest(TestReadOnlyApplicationFactory factory)
    : base(factory)
    {
    }

    [Fact]
    public async Task DomainOfInfluenceManagerShouldReturn()
    {
        var result = await GemeindeArneggElectionAdminClient.ListAttachmentAccessibleAsync(new ListAttachmentAccessiblePoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ChildTenantForParentDomainOfInfluenceShouldReturn()
    {
        var result = await GemeindeArneggElectionAdminClient.ListAttachmentAccessibleAsync(new ListAttachmentAccessiblePoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ContestManagerShouldReturn()
    {
        var result = await AbraxasElectionAdminClient.ListAttachmentAccessibleAsync(new ListAttachmentAccessiblePoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task PrintJobManagerShouldReturn()
    {
        var result = await AbraxasPrintJobManagerClient.ListAttachmentAccessibleAsync(new ListAttachmentAccessiblePoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedBundId,
        });
        result.PoliticalBusinesses_.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ForeignTenantShouldReturnEmpty()
    {
        var result = await StadtUzwilElectionAdminClient.ListAttachmentAccessibleAsync(new ListAttachmentAccessiblePoliticalBusinessesRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        result.PoliticalBusinesses_.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(PoliticalBusinessService.PoliticalBusinessServiceClient service)
        => await service.ListAsync(new ListPoliticalBusinessesRequest { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId });

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
