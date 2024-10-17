// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public interface IContestEVotingExportThrottler
{
    Task Acquire(CancellationToken ct = default);

    void Release();
}
