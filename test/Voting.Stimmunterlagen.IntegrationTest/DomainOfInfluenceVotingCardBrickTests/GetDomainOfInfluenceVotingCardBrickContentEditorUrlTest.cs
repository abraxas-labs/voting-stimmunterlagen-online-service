// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Lib.DmDoc.Exceptions;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardBrickTests;

public class GetDomainOfInfluenceVotingCardBrickContentEditorUrlTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient>
{
    public GetDomainOfInfluenceVotingCardBrickContentEditorUrlTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetContentEditorShouldReturn()
    {
        var result = await AbraxasElectionAdminClient.GetContentEditorUrlAsync(NewValidRequest());
        result.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task NotFoundShouldThrow()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.GetContentEditorUrlAsync(NewValidRequest(x => x.BrickId = 2)),
            StatusCode.Internal,
            nameof(DmDocException));
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient service)
    {
        await service.GetContentEditorUrlAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest NewValidRequest(Action<GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest>? customizer = null)
    {
        var req = new GetDomainOfInfluenceVotingCardBrickContentEditorUrlRequest
        {
            BrickId = 1,
            ContentId = 501,
        };
        customizer?.Invoke(req);
        return req;
    }
}
