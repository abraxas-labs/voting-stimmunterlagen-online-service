// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport;

public class VotingExportRenderServiceAdapter
{
    private readonly ILogger<VotingExportRenderServiceAdapter> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly VotingExportRenderServiceRegistry _renderServiceRegistry;

    public VotingExportRenderServiceAdapter(
        ILogger<VotingExportRenderServiceAdapter> logger,
        VotingExportRenderServiceRegistry renderServiceRegistry,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _renderServiceRegistry = renderServiceRegistry;
        _serviceProvider = serviceProvider;
    }

    public Task<FileModel> Render(VotingExportRenderContext context, CancellationToken ct)
    {
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ExportKey"] = context.Key,
            ["DomainOfInfluenceId"] = context.DomainOfInfluence.Id,
        });

        var renderer = _renderServiceRegistry.GetRenderService(context.Key, _serviceProvider) ?? throw new InvalidOperationException($"result render service for key {context.Key} not found");
        return renderer.Render(context, ct);
    }
}
