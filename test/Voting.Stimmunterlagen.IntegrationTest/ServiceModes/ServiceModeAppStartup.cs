// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voting.Lib.DmDoc;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Core.Mocks;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.IntegrationTest.Helpers;

namespace Voting.Stimmunterlagen.IntegrationTest.ServiceModes;

public class ServiceModeAppStartup : TestStartup
{
    public ServiceModeAppStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // required to seed mock data, but not provided in all service modes
        services.TryAddScoped<DomainOfInfluenceVotingCardLayoutManager>();
        services.TryAddScoped<TemplateManager>();
        services.TryAddScoped<ContestBuilder>();
        services.TryAddScoped<ContestOrderNumberStateBuilder>();
        services.TryAddScoped<ContestVotingCardLayoutBuilder>();
        services.TryAddScoped<DomainOfInfluenceVotingCardLayoutBuilder>();
        services.TryAddScoped<DomainOfInfluenceVotingCardConfigurationBuilder>();
        services.TryAddScoped<PoliticalBusinessPermissionBuilder>();
        services.TryAddScoped<ContestDomainOfInfluenceBuilder>();
        services.TryAddScoped<ContestCountingCircleBuilder>();
        services.TryAddScoped<DomainOfInfluenceCountingCircleBuilder>();
        services.TryAddSingleton<TemplateDataBuilder>();
        services.TryAddSingleton<ContestManager>();
        services.TryAddSingleton<IDmDocService, DmDocServiceMock>();
        services.TryAddSingleton(AppConfig.Api);
        services.TryAddSingleton(AppConfig.EventProcessor);
        services.AddMemoryCache();
        services
            .AddVotingLibObjectStorage(AppConfig.Api.ObjectStorage)
            .AddBucketClient<DomainOfInfluenceLogoStorage>();
    }
}
