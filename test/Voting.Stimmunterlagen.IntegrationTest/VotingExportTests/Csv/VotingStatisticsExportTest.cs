// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingExportTests.Csv;

public class VotingStatisticsExportTest : VotingCsvExportBaseTest
{
    public VotingStatisticsExportTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    protected override string NewRequestExpectedFileName => "Voting_Statistics_1240.csv";

    protected override GenerateVotingExportRequest NewRequest()
    {
        return new()
        {
            Key = VotingExportKeys.VotingStatistics,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureApprovedGemeindeArneggGuid,
        };
    }
}
