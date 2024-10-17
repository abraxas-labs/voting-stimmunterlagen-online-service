// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Models;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.ContestVotingCardLayoutTests;

public class GetContestVotingCardLayoutPdfPreviewTest : BaseReadOnlyGrpcTest<ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient>
{
    public GetContestVotingCardLayoutPdfPreviewTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var pdfPreview = await AbraxasElectionAdminClient.GetPdfPreviewAsync(new()
        {
            ContestId = ContestMockData.BundFutureId,
            VotingCardType = VotingCardType.Swiss,
        });
        pdfPreview.Pdf.ShouldBeAPdf();
    }

    [Fact]
    public Task ShouldReturnEmptyIfNotContestManager()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetPdfPreviewAsync(new()
            {
                ContestId = ContestMockData.BundFutureId,
                VotingCardType = VotingCardType.Swiss,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(ContestVotingCardLayoutService.ContestVotingCardLayoutServiceClient service)
    {
        await service.GetPdfPreviewAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
