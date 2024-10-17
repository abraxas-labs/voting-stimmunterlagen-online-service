// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Serialization;
using Voting.Lib.DmDoc.Serialization.Json;
using Voting.Lib.Ech;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmregister.Proto.V1.Services;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Core.Managers.Stimmregister;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.IntegrationTest.MockData.Mapping;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .AddVotingLibEventingMocks()
            .AddVotingLibObjectStorageMock()
            .AddVotingLibIamMocks()
            .AddDokConnectorMock()
            .AddMockedClock()
            .RemoveHostedServices()
            .AddMock<IVotingCardGeneratorThrottler, VotingCardGeneratorThrottlerMock>()
            .AddMock<IVotingCardStore, VotingCardStoreMock>()
            .AddMock<IContestEVotingExportThrottler, ContestEVotingExportThrottlerMock>()
            .AddMock<IContestEVotingStore, ContestEVotingStoreMock>()
            .AddMock<IVotingCardPrintFileStore, VotingCardPrintFileStoreMock>()
            .AddMock<IVotingCardPrintFileExportThrottler, VotingCardPrintFileExportThrottlerMock>()
            .AddMock<IEchMessageIdProvider, MockEchMessageIdProvider>()
            .AddMock<FilterService.FilterServiceClient>(StimmregisterClientMock.ConfigureFilterClientMock)
            .AddMock<IEVotingZipStorage, EVotingZipStorageMock>()
            .AddSingleton<TestMapper>()
            .AddSingleton<IDmDocDataSerializer, DmDocJsonDataSerializer>()
            .AddSingleton<IDmDocDraftCleanupQueue, DmDocDraftCleanupQueue>();

        var stimmregisterHttpClientMock = StimmregisterClientMock.CreateHttpClientMock();
        services.Configure<HttpClientFactoryOptions>(
            nameof(StimmregisterElectoralRegisterClient),
            options => options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = stimmregisterHttpClientMock));
    }

    protected override void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddMockedSecureConnectScheme();
}
