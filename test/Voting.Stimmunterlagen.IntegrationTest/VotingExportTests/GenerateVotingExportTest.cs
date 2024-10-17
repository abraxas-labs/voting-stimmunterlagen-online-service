// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingExportTests;

public class GenerateVotingExportTest : BaseWriteableDbRestTest
{
    private const string ExportEndpoint = "v1/voting-export";

    public GenerateVotingExportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldThrowNonDomainOfInfluenceManager()
    {
        var request = new GenerateVotingExportRequest
        {
            Key = VotingExportKeys.VotingJournal,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        };

        await AssertStatus(
            () => AbraxasElectionAdminClient.PostAsJsonAsync(ExportEndpoint, request),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsPrintJobManager()
    {
        var request = new GenerateVotingExportRequest
        {
            Key = VotingExportKeys.VotingJournal,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        };

        await AssertStatus(
            () => AbraxasPrintJobManagerClient.PostAsJsonAsync(ExportEndpoint, request),
            HttpStatusCode.Forbidden);
    }
}
