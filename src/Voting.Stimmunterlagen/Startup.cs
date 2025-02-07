// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Prometheus;
using Voting.Lib.Common.DependencyInjection;
using Voting.Lib.Grpc.Interceptors;
using Voting.Lib.Rest.Middleware;
using Voting.Lib.Rest.Swagger.DependencyInjection;
using Voting.Lib.Rest.Utils;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.MappingProfiles;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Interceptors;
using Voting.Stimmunterlagen.Middlewares;
using Voting.Stimmunterlagen.Services;
using ExceptionHandler = Voting.Stimmunterlagen.Middlewares.ExceptionHandler;
using ExceptionInterceptor = Voting.Stimmunterlagen.Interceptors.ExceptionInterceptor;

namespace Voting.Stimmunterlagen;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        AppConfig = configuration.Get<AppConfig>()!;
        _configuration = configuration;
    }

    protected AppConfig AppConfig { get; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(AppConfig);
        services.AddCertificatePinning(AppConfig.CertificatePinning);

        services.AddAutoMapper(typeof(Startup), typeof(ContestProfile));

        services.AddCore(AppConfig);
        services.AddData(AppConfig.Database, ConfigureDatabase);

        ConfigureHealthChecks(services.AddHealthChecks());
        ConfigureAuthentication(services.AddVotingLibIam(new() { BaseUrl = AppConfig.SecureConnectApi }, AppConfig.AuthStore));

        AddApiServices(services);

        services.AddVotingLibPrometheusAdapter(new() { Interval = AppConfig.PrometheusAdapterInterval });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMetricServer(AppConfig.MetricPort);

        app.UseRouting();
        UseApi(app);

        app.UseSwaggerGenerator();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapVotingHealthChecks(AppConfig.LowPriorityHealthCheckNames);
            MapEndpoints(endpoints);
        });
    }

    protected virtual void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddSecureConnectScheme(options =>
        {
            options.Audience = AppConfig.SecureConnect.Audience;
            options.Authority = AppConfig.SecureConnect.Authority;
            options.FetchRoleToken = true;
            options.LimitRolesToAppHeaderApps = false;
            options.ServiceAccount = AppConfig.SecureConnect.ServiceAccount;
            options.ServiceAccountPassword = AppConfig.SecureConnect.ServiceAccountPassword;
            options.ServiceAccountScopes = AppConfig.SecureConnect.ServiceAccountScopes;
        });

    protected virtual void ConfigureDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(
            AppConfig.Database.ConnectionString,
            ConfigureNpgsql);

    protected void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder options)
    {
        // use single query by default.
        options
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            .SetPostgresVersion(AppConfig.Database.Version);
    }

    private void ConfigureHealthChecks(IHealthChecksBuilder checks)
    {
        checks
            .AddDbContextCheck<DataContext>()
            .ForwardToPrometheus();

        if (AppConfig.EventProcessorModeEnabled)
        {
            checks.AddEventStore();
        }
    }

    private void AddApiServices(IServiceCollection services)
    {
        if (!AppConfig.ApiModeEnabled)
        {
            return;
        }

        if (AppConfig.Api.EnableGrpcWeb)
        {
            services.AddCors(_configuration);
        }

        services
            .AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddSingleton<MultipartRequestHelper>();

        services.AddGrpc(o =>
        {
            o.EnableDetailedErrors = AppConfig.Api.EnableDetailedErrors;
            o.Interceptors.Add<ExceptionInterceptor>();
            o.Interceptors.Add<RequestProtoValidatorInterceptor>();
            o.Interceptors.Add<LanguageInterceptor>();
            o.Interceptors.Add<AppModuleInterceptor>();
        });

        services.AddProtoValidators();
        services.AddScoped<AppContext>();
        services.AddGrpcReflection();
        services.AddSwaggerGenerator(_configuration);
        services.AddSecureConnectServiceAccount(AppConfig.SharedSecureConnectServiceAccountName, AppConfig.SharedSecureConnect);
    }

    private void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        if (!AppConfig.ApiModeEnabled)
        {
            return;
        }

        endpoints.MapControllers();
        endpoints.MapGrpcReflectionService();
        endpoints.MapGrpcService<ContestService>();
        endpoints.MapGrpcService<DomainOfInfluenceService>();
        endpoints.MapGrpcService<PoliticalBusinessService>();
        endpoints.MapGrpcService<StepService>();
        endpoints.MapGrpcService<AttachmentService>();
        endpoints.MapGrpcService<VoterListService>();
        endpoints.MapGrpcService<ContestVotingCardLayoutService>();
        endpoints.MapGrpcService<DomainOfInfluenceVotingCardLayoutService>();
        endpoints.MapGrpcService<DomainOfInfluenceVotingCardService>();
        endpoints.MapGrpcService<ElectoralRegisterService>();
        endpoints.MapGrpcService<VotingCardGeneratorJobService>();
        endpoints.MapGrpcService<ManualVotingCardGeneratorJobService>();
        endpoints.MapGrpcService<PrintJobService>();
        endpoints.MapGrpcService<ContestEVotingExportJobService>();
        endpoints.MapGrpcService<DomainOfInfluenceVotingCardBrickService>();
        endpoints.MapGrpcService<VotingCardPrintFileExportJobService>();
        endpoints.MapGrpcService<AdditionalInvoicePositionService>();
        endpoints.MapGrpcService<VoterListImportService>();
    }

    private void UseApi(IApplicationBuilder app)
    {
        if (!AppConfig.ApiModeEnabled)
        {
            return;
        }

        app.UseMiddleware<ExceptionHandler>();

        if (AppConfig.Api.EnableGrpcWeb)
        {
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
            app.UseCorsFromConfig();
        }

        app.UseHttpMetrics();
        app.UseGrpcMetrics();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<IamLoggingHandler>();
        app.UseSerilogRequestLoggingWithTraceabilityModifiers();
        app.UseMiddleware<LanguageMiddleware>();
        app.UseHeaderPropagation();
    }
}
