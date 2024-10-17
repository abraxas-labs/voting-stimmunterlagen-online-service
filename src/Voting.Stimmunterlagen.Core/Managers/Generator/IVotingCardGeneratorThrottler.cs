// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public interface IVotingCardGeneratorThrottler
{
    Task Acquire(CancellationToken ct = default);

    void Release();
}
