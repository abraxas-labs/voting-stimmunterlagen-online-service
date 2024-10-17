// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.Auth;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.DomainOfInfluenceVotingCardTests;

public class GetContestDomainOfInfluenceVotingCardConfigurationTest
    : BaseReadOnlyGrpcTest<DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient>
{
    public GetContestDomainOfInfluenceVotingCardConfigurationTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var configurations = await GemeindeArneggElectionAdminClient.GetConfigurationAsync(new GetDomainOfInfluenceVotingCardConfigurationRequest
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        configurations.MatchSnapshot();
    }

    [Fact]
    public Task ShouldThrowOtherTenant()
    {
        return AssertStatus(
            async () => await GemeindeArneggElectionAdminClient.GetConfigurationAsync(new GetDomainOfInfluenceVotingCardConfigurationRequest
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedStadtGossauId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(DomainOfInfluenceVotingCardService.DomainOfInfluenceVotingCardServiceClient service)
    {
        await service.GetConfigurationAsync(new());
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return Roles.PrintJobManager;
    }
}
