// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.EventProcessors;
using Voting.Stimmunterlagen.Core.Managers.EVoting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, AppConfig config)
    {
        return services
            .AddScoped<StepsBuilder>()
            .AddScoped<PrintJobBuilder>()
            .AddScoped<ContestEVotingExportJobBuilder>()
            .AddScoped<AttachmentBuilder>()
            .AddScoped<VoterListBuilder>()
            .AddEventProcessorServices(config)
            .AddApiServices(config);
    }
}
