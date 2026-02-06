// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Common.Net;
using Voting.Lib.Iam.Configuration;
using Voting.Lib.Iam.TokenHandling.ServiceToken;
using Voting.Stimmunterlagen.Data.Configuration;

namespace Voting.Stimmunterlagen.Core.Configuration;

public class AppConfig
{
    public const string SharedSecureConnectServiceAccountName = "SharedSecureConnect";

    public ServiceMode ServiceMode { get; set; } = ServiceMode.Hybrid;

    public PortConfig Ports { get; set; } = new();

    /// <summary>
    /// Gets or sets the port configuration for the metric endpoint.
    /// </summary>
    public ushort MetricPort { get; set; } = 9090;

    public EventProcessorConfig EventProcessor { get; set; } = new();

    public ApiConfig Api { get; set; } = new();

    public DataConfig Database { get; set; } = new();

    public SecureConnectConfiguration SecureConnect { get; set; } = new();

    public Uri? SecureConnectApi { get; set; }

    public SecureConnectServiceAccountOptions SharedSecureConnect { get; set; } = new();

    public long? CacheSizeLimit { get; set; } = 10_000_000; // 10MB

    public CertificatePinningConfig CertificatePinning { get; set; } = new();

    /// <summary>
    /// Gets or sets the CORS config options used within the <see cref="Voting.Lib.Common.DependencyInjection.ApplicationBuilderExtensions"/>
    /// to configure the CORS middleware from <see cref="Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions"/>.
    /// </summary>
    public CorsConfig Cors { get; set; } = new();

    public TimeSpan PrometheusAdapterInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the health check names of all health checks which are considered as non mission-critical
    /// (if any of them is unhealthy the system may still operate but in a degraded state).
    /// These health checks are monitored separately.
    /// If <c>null</c>, the health checks marked with a <see cref="HealthChecks.Tags.LowPriority"/> tag are considered low priority.
    /// </summary>
    public HashSet<string>? LowPriorityHealthCheckNames { get; set; }

    public bool ApiModeEnabled
        => (ServiceMode & ServiceMode.Api) != 0;

    public bool EventProcessorModeEnabled
        => (ServiceMode & ServiceMode.EventProcessor) != 0;

    /// <summary>
    /// Gets or sets the auth store configuration.
    /// </summary>
    public AuthStoreConfig AuthStore { get; set; } = new();
}
