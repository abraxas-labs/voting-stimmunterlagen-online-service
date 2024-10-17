// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.DokConnector.Service;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class DokConnectContestEVotingStore : IContestEVotingStore
{
    private readonly IDokConnector _connector;
    private readonly ILogger<DokConnectContestEVotingStore> _logger;

    public DokConnectContestEVotingStore(IDokConnector connector, ILogger<DokConnectContestEVotingStore> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    public async Task SaveContestEVotingExport(string fileName, byte[] content, string messageId, CancellationToken ct)
    {
        await using var ms = new MemoryStream(content);
        _logger.LogDebug("uploading contest e-voting export archive to {MessageId}/{FileName}", messageId, fileName);
        await _connector.Upload(messageId, fileName, ms, ct);
    }
}
