// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.DokConnector.Service;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class DokConnectVotingCardPrintFileStore : IVotingCardPrintFileStore
{
    private readonly IDokConnector _connector;
    private readonly ILogger<DokConnectVotingCardPrintFileStore> _logger;

    public DokConnectVotingCardPrintFileStore(IDokConnector connector, ILogger<DokConnectVotingCardPrintFileStore> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    public async Task SavePrintFile(string fileName, byte[] content, string messageId, CancellationToken ct)
    {
        await using var ms = new MemoryStream(content);
        _logger.LogDebug("uploading print file to {MessageId}/{FileName}", messageId, fileName);
        await _connector.Upload(messageId, fileName, ms, ct);
    }
}
