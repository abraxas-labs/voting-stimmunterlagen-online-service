// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public interface IVotingCardStore
{
    Task SaveVotingCards(string fileName, Stream content, string messageId, CancellationToken ct);
}
