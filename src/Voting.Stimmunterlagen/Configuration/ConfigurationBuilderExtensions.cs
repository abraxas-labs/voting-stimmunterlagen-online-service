// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    private const string TestDomainOfInfluencesConfigPath = "TestDomainOfInfluencesConfig.json";

    public static IConfigurationBuilder AddTestDomainOfInfluences(this IConfigurationBuilder builder)
    {
        builder.Sources.Remove(builder.Sources.FirstOrDefault(c => c.GetType() == typeof(EnvironmentVariablesConfigurationSource))!);

        builder
            .AddJsonFile(TestDomainOfInfluencesConfigPath)
            .AddEnvironmentVariables(); // ensure that environment variables get added to configuration after the json is added
        return builder;
    }
}
