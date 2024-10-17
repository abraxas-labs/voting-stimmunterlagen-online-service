// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardLayoutTests;

public class GetDomainOfInfluenceVotingCardLayoutPdfPreviewTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient>
{
    public GetDomainOfInfluenceVotingCardLayoutPdfPreviewTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWorkAsContestManager()
    {
        var pdfPreview = await AbraxasElectionAdminClient.GetPdfPreviewAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            VotingCardType = VotingCardType.Swiss,
        });
        pdfPreview.Pdf.ShouldBeAPdf();
    }

    [Fact]
    public async Task ShouldWorkAsDoiManager()
    {
        var pdfPreview = await GemeindeArneggElectionAdminClient.GetPdfPreviewAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureGemeindeArneggId,
            VotingCardType = VotingCardType.Swiss,
        });
        pdfPreview.Pdf.ShouldBeAPdf();
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardLayoutService.DomainOfInfluenceVotingCardLayoutServiceClient service)
    {
        await service.GetPdfPreviewAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
