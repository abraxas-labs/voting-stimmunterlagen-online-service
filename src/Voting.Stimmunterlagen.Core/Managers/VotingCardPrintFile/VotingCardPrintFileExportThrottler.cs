// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class VotingCardPrintFileExportThrottler : IVotingCardPrintFileExportThrottler, IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public VotingCardPrintFileExportThrottler(ApiConfig config)
    {
        _semaphore = new SemaphoreSlim(config.VotingCardPrintFileExport.ParallelTasks, config.VotingCardPrintFileExport.ParallelTasks);
    }

    public void Dispose() => _semaphore.Dispose();

    public Task Acquire(CancellationToken ct = default) => _semaphore.WaitAsync(ct);

    public void Release() => _semaphore.Release();
}
