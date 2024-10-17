// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using FluentValidation;
using Voting.Stimmunterlagen.Core.Managers.VotingExport.RenderServices;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport;

public class VotingExportRenderServiceRegistry
{
    private readonly IDictionary<string, Type> _resultRenderServices = new Dictionary<string, Type>();

    public void Add<TRenderer>(string key)
    {
        if (_resultRenderServices.ContainsKey(key))
        {
            throw new ValidationException($"Result renderer for template key {key} already registered");
        }

        _resultRenderServices.Add(key, typeof(TRenderer));
    }

    internal IVotingRenderService? GetRenderService(string key, IServiceProvider serviceProvider)
    {
        return _resultRenderServices.TryGetValue(key, out var resultRenderServiceType)
            ? serviceProvider.GetService(resultRenderServiceType) as IVotingRenderService
            : null;
    }
}
