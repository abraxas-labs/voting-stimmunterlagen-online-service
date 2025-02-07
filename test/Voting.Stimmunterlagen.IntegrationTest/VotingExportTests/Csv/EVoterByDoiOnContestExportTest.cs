// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http;
using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;
using Voting.Stimmunterlagen.IntegrationTest.MockData;
using Voting.Stimmunterlagen.Models.Request;

namespace Voting.Stimmunterlagen.IntegrationTest.VotingExportTests.Csv;

public class EVoterByDoiOnContestExportTest : VotingCsvExportBaseTest
{
    public EVoterByDoiOnContestExportTest(TestApplicationFactory factory)
    : base(factory)
    {
    }

    protected override HttpClient TestClient => AbraxasElectionAdminClient;

    protected override string NewRequestExpectedFileName => "E_Voters_By_Doi_On_Contest_11.01.2020.csv";

    protected override GenerateVotingExportRequest NewRequest()
    {
        return new()
        {
            Key = VotingExportKeys.EVoterByDoiOnContest,
            DomainOfInfluenceId = DomainOfInfluenceMockData.ContestBundFutureBundGuid,
        };
    }
}
