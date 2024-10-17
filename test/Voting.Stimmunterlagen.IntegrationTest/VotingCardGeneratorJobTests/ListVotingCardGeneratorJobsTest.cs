// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardGeneratorJobTests;

public class ListVotingCardGeneratorJobsTest : BaseReadOnlyGrpcTest<
    VotingCardGeneratorJobsService.VotingCardGeneratorJobsServiceClient>
{
    public ListVotingCardGeneratorJobsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForVotingDocuments()
    {
        var jobs = await GemeindeArneggElectionAdminClient.ListJobsAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        jobs.MatchSnapshot();
    }

    [Fact]
    public async Task ListForPrintJobManagement()
    {
        var jobs = await AbraxasPrintJobManagerClient.ListJobsAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        jobs.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowForPrintJobManagementIfInvalidDoi()
    {
        await AssertStatus(
            async () => await AbraxasPrintJobManagerClient.ListJobsAsync(new()
            { DomainOfInfluenceId = "cecb9be3-462f-4412-a023-f76a583ca0d2" }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(VotingCardGeneratorJobsService.VotingCardGeneratorJobsServiceClient service)
    {
        await service.ListJobsAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
