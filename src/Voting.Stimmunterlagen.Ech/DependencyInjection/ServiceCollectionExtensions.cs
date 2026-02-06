// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Ech.Configuration;
using Voting.Lib.Ech.Ech0045_4_0.DependencyInjection;
using Voting.Lib.Ech.Ech0045_6_0.DependencyInjection;
using Voting.Stimmunterlagen.Data.Models;
using Voting.Stimmunterlagen.Ech.Converter;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEch(this IServiceCollection services, EchConfig config)
    {
        return services
            .AddVotingLibEch(config)
            .AddEch0045V4()
            .AddEch0045V6()
            .AddSingleton<Ech0045Service>()
            .AddKeyedSingleton<IEch0045Converter, Ech0045_4_0_Converter>(Ech0045Version.V4)
            .AddKeyedSingleton<IEch0045Converter, Ech0045_6_0_Converter>(Ech0045Version.V6);
    }
}
