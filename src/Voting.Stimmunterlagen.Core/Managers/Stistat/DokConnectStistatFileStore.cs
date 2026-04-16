// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.DokConnector.Service;

namespace Voting.Stimmunterlagen.Core.Managers.Stistat;

public class DokConnectStistatFileStore : IStistatFileStore
{
    private readonly IDokConnector _connector;
    private readonly ILogger<DokConnectStistatFileStore> _logger;

    public DokConnectStistatFileStore(IDokConnector connector, ILogger<DokConnectStistatFileStore> logger)
    {
        _connector = connector;
        _logger = logger;
    }

    public async Task Save(string fileName, Stream content, string messageType, CancellationToken ct)
    {
        _logger.LogInformation("Uploading stistat export to {MessageType}:{FileName}", messageType, fileName);
        await _connector.Upload(messageType, fileName, content, ct);
    }
}
