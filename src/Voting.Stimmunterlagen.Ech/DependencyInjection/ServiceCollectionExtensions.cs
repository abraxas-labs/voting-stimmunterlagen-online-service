// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Ech.Configuration;
using Voting.Lib.Ech.Ech0045_4_0.DependencyInjection;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEch(this IServiceCollection services, EchConfig config)
    {
        return services
            .AddVotingLibEch(config)
            .AddEch0045()
            .AddSingleton<EchService>();
    }
}
