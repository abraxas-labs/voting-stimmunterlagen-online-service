// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.IntegrationTest.MockData;

public static class MockDataSeeder
{
    public static async Task Seed(Func<Func<IServiceProvider, Task>, Task> runScoped)
    {
        await ContestOrderNumberStatesMockData.Seed(runScoped);
        await TemplateMockData.Seed(runScoped);
        await CountingCircleMockData.Seed(runScoped);
        await DomainOfInfluenceMockData.Seed(runScoped);
        await ContestMockData.SeedStage1(runScoped);
        await VoteMockData.Seed(runScoped);
        await MajorityElectionMockData.Seed(runScoped);
        await SecondaryMajorityElectionMockData.Seed(runScoped);
        await ProportionalElectionMockData.Seed(runScoped);
        await ContestMockData.SeedStage2(runScoped);
        await StepMockData.Seed(runScoped);
        await ContestVotingCardLayoutMockData.Seed(runScoped);
        await DomainOfInfluenceVotingCardLayoutMockData.Seed(runScoped);
        await AttachmentMockData.Seed(runScoped);
        await VoterListImportMockData.Seed(runScoped);
        await VoterListMockData.Seed(runScoped);
        await PrintJobMockData.Seed(runScoped);
        await DomainOfInfluenceVotingCardConfigurationMockData.Seed(runScoped);
        await VotingCardGeneratorJobMockData.Seed(runScoped);
        await ManualVotingCardGeneratorJobMockData.Seed(runScoped);
        await CantonSettingsMockData.Seed(runScoped);
        await ContestEVotingExportJobMockData.Seed(runScoped);
        await VotingCardPrintFileExportJobMockData.Seed(runScoped);
        await AdditionalInvoicePositionMockData.Seed(runScoped);
    }

    public static class SecureConnectTenantIds
    {
        public const string Abraxas = "SC-ABX";

        public const string StaatskanzleiStGallen = "SC-SG";

        public const string StadtUzwil = "SC-MU-UZ";
        public const string StadtGossau = "SC-MU-GO";
        public const string GemeindeArnegg = "SC-MU-AG";
        public const string Auslandschweizer = "SC-MU-AS";

        public const string Unknown = "UNKNOWN";
    }
}
