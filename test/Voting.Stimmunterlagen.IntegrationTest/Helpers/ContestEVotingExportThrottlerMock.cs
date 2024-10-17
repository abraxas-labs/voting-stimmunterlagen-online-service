// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmunterlagen.Core.Managers.EVoting;

namespace Voting.Stimmunterlagen.IntegrationTest.Helpers;

public class ContestEVotingExportThrottlerMock : IContestEVotingExportThrottler
{
    public bool ShouldBlock { get; set; }

    public int BlockedCount { get; set; }

    public async Task Acquire(CancellationToken ct = default)
    {
        if (!ShouldBlock)
        {
            return;
        }

        BlockedCount++;
        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        finally
        {
            BlockedCount--;
        }
    }

    public void Release()
    {
    }
}
