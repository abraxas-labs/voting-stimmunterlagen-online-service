// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Voting.Lib.Testing.Utils;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Proto.V1;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingCardPrintFileTests;

public class ListVotingCardPrintFileExportJobsTest : BaseReadOnlyGrpcTest<
    VotingCardPrintFileExportJobService.VotingCardPrintFileExportJobServiceClient>
{
    public ListVotingCardPrintFileExportJobsTest(TestReadOnlyApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ListForPrintJobManagement()
    {
        var jobs = await AbraxasPrintJobManagerClient.ListAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        jobs.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnIfElectionAdminAndContestManager()
    {
        var jobs = await AbraxasElectionAdminClient.ListAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        jobs.MatchSnapshot();
    }

    [Fact]
    public async Task ShouldReturnEmptyIfElectionAdminAndNotContestManager()
    {
        var jobs = await StadtGossauElectionAdminClient.ListAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
        jobs.Jobs.Should().HaveCount(0);
    }

    protected override async Task AuthorizationTestCall(VotingCardPrintFileExportJobService.VotingCardPrintFileExportJobServiceClient service)
    {
        await service.ListAsync(new()
        {
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggId,
        });
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
    }
}
