// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class ContestEVotingExportThrottler : IContestEVotingExportThrottler, IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public ContestEVotingExportThrottler(ApiConfig config)
    {
        _semaphore = new SemaphoreSlim(config.ContestEVotingExport.ParallelTasks, config.ContestEVotingExport.ParallelTasks);
    }

    public Task Acquire(CancellationToken ct = default) => _semaphore.WaitAsync(ct);

    public void Release() => _semaphore.Release();

    public void Dispose() => _semaphore.Dispose();
}
