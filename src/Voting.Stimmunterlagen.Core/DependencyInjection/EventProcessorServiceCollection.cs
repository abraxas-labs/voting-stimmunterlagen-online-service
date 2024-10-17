// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Eventing;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Utils;
using ProtoBasis = Abraxas.Voting.Basis.Events.V1.Data;

namespace Microsoft.Extensions.DependencyInjection;

internal static class EventProcessorServiceCollection
{
    internal static IServiceCollection AddEventProcessorServices(this IServiceCollection services, AppConfig config)
    {
        if (!config.EventProcessorModeEnabled)
        {
            return services;
        }

        return services
            .AddSingleton(config.EventProcessor)
            .AddVotingLibEventing(config.EventProcessor.EventStore, typeof(ProtoBasis.EventInfo).Assembly).AddSubscription<EventProcessorScope>(WellKnownStreams.CategoryVoting).Services
            .AddScoped<PoliticalBusinessPermissionBuilder>()
            .AddScoped<ContestDomainOfInfluenceBuilder>()
            .AddScoped<ContestCountingCircleBuilder>()
            .AddScoped<DomainOfInfluenceCountingCircleBuilder>()
            .AddScoped<DomainOfInfluenceCantonDefaultsBuilder>()
            .AddScoped<ContestBuilder>()
            .AddScoped<ContestOrderNumberStateBuilder>()
            .AddScoped<ContestVotingCardLayoutBuilder>()
            .AddScoped<DomainOfInfluenceVotingCardLayoutBuilder>()
            .AddScoped<DomainOfInfluenceVotingCardConfigurationBuilder>();
    }
}
