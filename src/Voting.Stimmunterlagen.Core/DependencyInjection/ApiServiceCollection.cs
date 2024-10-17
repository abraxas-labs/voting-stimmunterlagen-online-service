// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Net.Http.Headers;
using Voting.Lib.Common;
using Voting.Lib.DmDoc;
using Voting.Lib.DocPipe;
using Voting.Lib.Grpc.Extensions;
using Voting.Lib.Scheduler;
using Voting.Stimmunterlagen.Core.Configuration;
using Voting.Stimmunterlagen.Core.HostedServices;
using Voting.Stimmunterlagen.Core.Managers;
using Voting.Stimmunterlagen.Core.Managers.EVoting;
using Voting.Stimmunterlagen.Core.Managers.Generator;
using Voting.Stimmunterlagen.Core.Managers.Invoice;
using Voting.Stimmunterlagen.Core.Managers.Steps;
using Voting.Stimmunterlagen.Core.Managers.Stimmregister;
using Voting.Stimmunterlagen.Core.Managers.Templates;
using Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;
using Voting.Stimmunterlagen.Core.Managers.VotingExport;
using Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;
using Voting.Stimmunterlagen.Core.ObjectStorage;
using Voting.Stimmunterlagen.Core.Utils;
using Voting.Stimmunterlagen.Data.Models;

#if DEBUG
using Voting.Stimmunterlagen.Core.Mocks;
#endif

namespace Microsoft.Extensions.DependencyInjection;

internal static class ApiServiceCollection
{
    internal static IServiceCollection AddApiServices(this IServiceCollection services, AppConfig config)
    {
        if (!config.ApiModeEnabled)
        {
            return services;
        }

        return services
            .AddSingleton(config.Api)
            .AddSingleton<AttributeValidator>()
            .AddScoped<UserManager>()
            .AddScoped<DomainOfInfluenceManager>()
            .AddScoped<PoliticalBusinessManager>()
            .AddScoped<AttachmentManager>()
            .AddScoped<VoterListManager>()
            .AddScoped<VoterListImportManager>()
            .AddScoped<TemplateManager>()
            .AddScoped<ManualVotingCardGeneratorJob>()
            .AddScoped<ContestVotingCardLayoutManager>()
            .AddScoped<DomainOfInfluenceVotingCardLayoutManager>()
            .AddScoped<DomainOfInfluenceVotingCardManager>()
            .AddScoped<StepManager>()
            .AddScoped<ISingleStepManager, ApprovePoliticalBusinessesStepManager>()
            .AddScoped<ISingleStepManager, ApproveContestStepManager>()
            .AddScoped<ISingleStepManager, GenerateVotingCardsStepManager>()
            .AddScoped<ISingleStepManager, AttachmentsStepManager>()
            .AddScoped<ISingleStepManager, VoterListsStepManager>()
            .AddScoped<ISingleStepManager, EVotingStepManager>()
            .AddSingleton<IVotingCardGeneratorThrottler, VotingCardGeneratorThrottler>()
            .AddScoped<VotingCardGenerator>()
            .AddScoped<VotingCardGeneratorJobBuilder>()
            .AddScoped<VotingCardGeneratorLauncher>()
            .AddScoped<LanguageManager>()
            .AddScoped<VotingCardGeneratorJobManager>()
            .AddScoped<ManualVotingCardGeneratorJobManager>()
            .AddScoped<PrintJobManager>()
            .AddScoped<ContestManager>()
            .AddScoped<ContestEVotingExportJobManager>()
            .AddScoped<ContestEVotingExportGenerator>()
            .AddScoped<ContestEVotingExportJobLauncher>()
            .AddSingleton<IContestEVotingExportThrottler, ContestEVotingExportThrottler>()
            .AddScoped<EVotingContestBuilder>()
            .AddScoped<DomainOfInfluenceVotingCardBrickManager>()
            .AddScoped<CsvService>()
            .AddScoped<VotingCardPrintFileBuilder>()
            .AddSingleton<IVotingCardPrintFileExportThrottler, VotingCardPrintFileExportThrottler>()
            .AddScoped<VotingCardPrintFileExportGenerator>()
            .AddScoped<VotingCardPrintFileExportJobBuilder>()
            .AddScoped<VotingCardPrintFileExportJobLauncher>()
            .AddScoped<VotingCardPrintFileExportJobManager>()
            .AddScoped<AttachmentCategorySummaryBuilder>()
            .AddScoped<ElectoralRegisterManager>()
            .AddScoped<AdditionalInvoicePositionManager>()
            .AddScoped<InvoiceExportManager>()
            .AddScoped<InvoiceFileBuilder>()
            .AddScoped<InvoiceFileEntriesBuilder>()
            .AddScoped<VotingExportManager>()
            .AddVotingExports()
            .AddEch(config.Api.Ech)
            .AddDmDocOrMock(config.Api.DmDoc)
            .AddDocPipeOrMock(config.Api.DocPipe)
            .AddDokConnector(config)
            .AddSingleton<TemplateDataBuilder>()
            .AddScheduledJob<VotingCardGeneratorScheduler>(config.Api.VotingCardGenerator.Scheduler)
            .AddScheduledJob<ContestEVotingExportScheduler>(config.Api.ContestEVotingExport.Scheduler)
            .AddScheduledJob<VotingCardPrintFileExportScheduler>(config.Api.VotingCardPrintFileExport.Scheduler)
            .AddObjectStorage(config)
            .AddMemoryCache(x => x.SizeLimit = config.CacheSizeLimit)
            .AddSystemClock()
            .AddStimmregister(config.Api.Stimmregister)
            .AddImageProcessing()
            .AddAbraxasHeaderPropagation(opts => opts.Headers.Add(HeaderNames.Authorization));
    }

    private static IServiceCollection AddStimmregister(this IServiceCollection services, StimmregisterConfig config)
    {
        services.AddGrpcClient<Voting.Stimmregister.Proto.V1.Services.FilterService.FilterServiceClient>(opts => opts.Address = config.GrpcEndpoint)
            .PassThroughUserTokenAndAuthInfo()
            .ConfigureGrpcPrimaryHttpMessageHandler(config.Mode)
            .ConfigureChannel(o => o.UnsafeUseInsecureChannelCallCredentials = config.UseUnsafeInsecureChannelCallCredentials);

        services.AddHttpClient<StimmregisterElectoralRegisterClient>(c => c.BaseAddress = config.RestEndpoint)
            .AddHeaderPropagation(x => x.Headers.Add(HeaderNames.Authorization))
            .AddAbraxasHeaderPropagation();
        return services;
    }

    private static IServiceCollection AddDmDocOrMock(this IServiceCollection services, DmDocConfig config)
    {
#if !RELEASE
        if (config.EnableMock)
        {
            return services
                .AddSingleton<IDmDocService, DmDocServiceMock>()
                .AddSingleton<IDmDocDraftCleanupQueue, DmDocDraftCleanupQueueMock>();
        }
#endif

        return services
            .AddScoped<IDmDocUserNameProvider, DmDocUserNameProvider>()
            .AddDmDoc(config);
    }

    private static IServiceCollection AddDocPipeOrMock(this IServiceCollection services, DocPipeConfig config)
    {
        services.AddSingleton(config);

#if !RELEASE
        if (config.EnableMock)
        {
            return services.AddScoped<IDocPipeService, DocPipeServiceMock>();
        }
#endif

        return services.AddDocPipe(config);
    }

    private static IServiceCollection AddDokConnector(this IServiceCollection services, AppConfig config)
    {
#if !RELEASE
        if (config.Api.SaveConnectFilesToFileSystem)
        {
            return services
                .AddSingleton<IVotingCardStore, FileSystemVotingCardStore>()
                .AddSingleton<IContestEVotingStore, FileSystemContestEVotingStore>()
                .AddSingleton<IVotingCardPrintFileStore, FileSystemVotingCardPrintFileStore>();
        }
#endif

        services
            .AddSingleton<IVotingCardStore, DokConnectVotingCardStore>()
            .AddSingleton<IContestEVotingStore, DokConnectContestEVotingStore>()
            .AddSingleton<IVotingCardPrintFileStore, DokConnectVotingCardPrintFileStore>()
            .AddEaiDokConnector(config.Api.DokConnector).AddSecureConnectServiceToken(AppConfig.SharedSecureConnectServiceAccountName);

        return services;
    }

    private static IServiceCollection AddObjectStorage(this IServiceCollection services, AppConfig config)
    {
        services
            .AddVotingLibObjectStorage(config.Api.ObjectStorage)
            .AddBucketClient<DomainOfInfluenceLogoStorage>()
            .AddBucketClient<EVotingZipStorage>();

        services.AddSingleton<IEVotingZipStorage, EVotingZipStorage>();
        return services;
    }

    private static IServiceCollection AddVotingExports(this IServiceCollection services)
    {
        var registry = new VotingExportRenderServiceRegistry();

        return services
            .AddVotingExportRenderService<StatisticsByReligionVotingRenderService>(registry, VotingExportKeys.StatisticsByReligion)
            .AddVotingExportRenderService<VotingJournalVotingRenderService>(registry, VotingExportKeys.VotingJournal)
            .AddVotingExportRenderService<VotingStatisticsVotingRenderService>(registry, VotingExportKeys.VotingStatistics)
            .AddSingleton(registry)
            .AddScoped<VotingExportRenderServiceAdapter>();
    }

    private static IServiceCollection AddVotingExportRenderService<TRender>(this IServiceCollection services, VotingExportRenderServiceRegistry registry, string key)
        where TRender : class, IVotingRenderService
    {
        registry.Add<TRender>(key);
        return services.AddScoped<TRender>();
    }
}
