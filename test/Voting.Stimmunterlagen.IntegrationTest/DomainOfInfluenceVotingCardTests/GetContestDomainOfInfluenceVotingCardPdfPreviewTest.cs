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
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardTests;

public class GetContestDomainOfInfluenceVotingCardPdfPreviewTest
    : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient>
{
    public GetContestDomainOfInfluenceVotingCardPdfPreviewTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var pdf = await GemeindeArneggElectionAdminClient.GetPdfPreviewAsync(new GetDomainOfInfluenceVotingCardPdfPreviewRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            VotingCardType = VotingCardType.Swiss,
        });
        pdf.Pdf.ShouldBeAPdf();
    }

    [Fact]
    public Task ShouldThrowOtherTenant()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetPdfPreviewAsync(new GetDomainOfInfluenceVotingCardPdfPreviewRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
                VotingCardType = VotingCardType.Swiss,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient service)
    {
        await service.GetPdfPreviewAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
