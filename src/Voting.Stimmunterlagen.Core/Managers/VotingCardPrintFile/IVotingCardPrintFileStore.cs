// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public interface IVotingCardPrintFileStore
{
    Task SavePrintFile(string fileName, byte[] content, string messageId, CancellationToken ct);
}
