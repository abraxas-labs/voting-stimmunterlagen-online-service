// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Voting.Lib.Testing;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class TestApplicationFactory<TStartup> : BaseTestApplicationFactory<TStartup>
    where TStartup : class
{
    public override HttpClient CreateHttpClient(bool authorize, string? tenant, string? userId, string[]? roles, IEnumerable<(string, string)>? additionalHeaders = null)
    {
        var httpClient = base.CreateHttpClient(authorize, tenant, userId, roles);
        httpClient.DefaultRequestHeaders.Add("x-language", "de");
        return httpClient;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration((_, config) => config.AddTestDomainOfInfluences())
            .UseSolutionRelativeContentRoot("src/Voting.Stimmunterlagen");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder()
            .UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration((_, config) =>
            {
                // we deploy our config with the docker image, no need to watch for changes
                foreach (var source in config.Sources.OfType<JsonConfigurationSource>())
                {
                    source.ReloadOnChange = false;
                }
            });
    }

    protected async Task RunScoped(Func<IServiceProvider, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }
}

public class TestApplicationFactory : TestApplicationFactory<TestStartup>
{
}
