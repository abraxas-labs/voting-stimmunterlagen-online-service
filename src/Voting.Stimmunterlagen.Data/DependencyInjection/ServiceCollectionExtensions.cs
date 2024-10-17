// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Interceptors;
using Voting.Stimmunterlagen.Data;
using Voting.Stimmunterlagen.Data.Configuration;
using Voting.Stimmunterlagen.Data.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddData(
        this IServiceCollection services,
        DataConfig config,
        Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<DataContext>((serviceProvider, db) =>
        {
            if (config.EnableDetailedErrors)
            {
                db.EnableDetailedErrors();
            }

            if (config.EnableSensitiveDataLogging)
            {
                db.EnableSensitiveDataLogging();
            }

            if (config.EnableMonitoring)
            {
                db.AddInterceptors(serviceProvider.GetRequiredService<DatabaseQueryMonitoringInterceptor>());
            }

            db.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            optionsBuilder(db);
        });

        if (config.EnableMonitoring)
        {
            services.AddDataMonitoring(config.Monitoring);
        }

        return services
            .AddVotingLibDatabase<DataContext>()
            .AddTransient(typeof(IDbRepository<>), typeof(DbRepository<>))
            .AddTransient<ContestDomainOfInfluenceRepo>()
            .AddTransient<DomainOfInfluenceRepo>()
            .AddTransient<ContestCountingCircleRepo>()
            .AddTransient<AttachmentRepo>()
            .AddTransient<DomainOfInfluenceAttachmentCountRepo>()
            .AddTransient<PoliticalBusinessAttachmentEntryRepo>()
            .AddTransient<PoliticalBusinessVoterListEntryRepo>()
            .AddTransient<VoterRepo>()
            .AddTransient<TemplateDataContainerRepo>()
            .AddTransient<ContestRepo>();
    }
}
