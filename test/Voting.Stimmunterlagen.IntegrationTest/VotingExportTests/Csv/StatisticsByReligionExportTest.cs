// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;
using Xunit;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingExportTests.Csv;

public class StatisticsByReligionExportTest : VotingCsvExportBaseTest
{
    public StatisticsByReligionExportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected override string NewRequestExpectedFileName => "Statistics_By_Religion_1240.csv";

    [Fact]
    public Task TestCsvWithVoterListFilter()
    {
        var request = NewRequest();
        request.VoterListId = VoterListMockData.BundFutureApprovedGemeindeArneggSwissGuid;
        return TestCsvSnapshot(request, "Statistics_By_Religion_1240_Stimmregister Schweizer Arnegg.csv", "VoterList");
    }

    protected override GenerateVotingExportRequest NewRequest()
    {
        return new()
        {
            Key = VotingExportKeys.StatisticsByReligion,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        };
    }
}
