// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class VotingCardGeneratorThrottler : IVotingCardGeneratorThrottler, IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public VotingCardGeneratorThrottler(ApiConfig config)
    {
        _semaphore = new SemaphoreSlim(config.VotingCardGenerator.ParallelTasks, config.VotingCardGenerator.ParallelTasks);
    }

    public void Dispose() => _semaphore.Dispose();

    public Task Acquire(CancellationToken ct = default) => _semaphore.WaitAsync(ct);

    public void Release() => _semaphore.Release();
}
