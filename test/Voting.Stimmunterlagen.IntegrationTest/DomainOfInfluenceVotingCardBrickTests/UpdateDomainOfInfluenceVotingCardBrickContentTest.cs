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

public class UpdateDomainOfInfluenceVotingCardBrickContentTest : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient>
{
    public UpdateDomainOfInfluenceVotingCardBrickContentTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateContentShouldReturn()
    {
        var result = await AbraxasElectionAdminClient.UpdateContentAsync(NewValidRequest());
        result.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task NotFoundShouldThrow()
    {
        await AssertStatus(
            async () => await AbraxasElectionAdminClient.UpdateContentAsync(NewValidRequest(x => x.ContentId = 2)),
            StatusCode.Internal,
            nameof(DmDocException));
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardBrickService.DomainOfInfluenceVotingCardBrickServiceClient service)
    {
        await service.UpdateContentAsync(NewValidRequest());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }

    private static UpdateDomainOfInfluenceVotingCardBrickContentRequest NewValidRequest(Action<UpdateDomainOfInfluenceVotingCardBrickContentRequest>? customizer = null)
    {
        var req = new UpdateDomainOfInfluenceVotingCardBrickContentRequest
        {
            ContentId = 501,
            Content = "new brick content",
        };
        customizer?.Invoke(req);
        return req;
    }
}
