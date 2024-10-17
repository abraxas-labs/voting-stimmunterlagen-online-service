// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.DokConnector.Service;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class DokConnectVotingCardStore : IVotingCardStore
{
    private readonly IDokConnector _connector;
    private readonly ILogger<DokConnectVotingCardStore> _logger;

    public DokConnectVotingCardStore(IDokConnector connector, ILogger<DokConnectVotingCardStore> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    public async Task SaveVotingCards(string fileName, Stream content, string messageId, CancellationToken ct)
    {
        _logger.LogDebug("uploading voting card file to {MessageId}/{FileName}", messageId, fileName);
        await _connector.Upload(messageId, fileName, content, ct);
    }
}
