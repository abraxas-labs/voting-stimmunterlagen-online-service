// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Snapper;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Voting.Stimmunterlagen.Proto.V1.Requests;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.PrintJobTests;

public class GetPrintJobTest : BaseReadOnlyGrpcTest<PrintJobService.PrintJobServiceClient>
{
    public GetPrintJobTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldReturn()
    {
        var job = await AbraxasPrintJobManagerClient.GetAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        job.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnIfElectionAdminAndContestManager()
    {
        var job = await AbraxasElectionAdminClient.GetAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        job.ShouldMatchSnapshot();
    }

    [Fact]
    public async Task ShouldThrowIfElectionAdminAndNotContestManager()
    {
        await AssertStatus(
            async () => await StadtGossauElectionAdminClient.GetAsync(new()
            {
                DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(PrintJobService.PrintJobServiceClient service)
    {
        await service.GetAsync(new GetPrintJobRequest { DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundId });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
