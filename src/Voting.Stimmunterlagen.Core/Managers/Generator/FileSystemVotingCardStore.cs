// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.Generator;

public class FileSystemVotingCardStore : IVotingCardStore
{
    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemVotingCardStore> _logger;

    public FileSystemVotingCardStore(ApiConfig config, ILogger<FileSystemVotingCardStore> logger)
    {
        _logger = logger;
        _baseDirectory = Path.GetFullPath(config.VotingCardGenerator.OutDirectoryPath);
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task SaveVotingCards(string fileName, Stream content, string messageId, CancellationToken ct)
    {
        var targetDirectory = Path.Combine(_baseDirectory, messageId);
        Directory.CreateDirectory(targetDirectory);
        var targetFile = Path.Combine(targetDirectory, fileName);
        _logger.LogDebug("writing voting card file to {MessageId}/{TargetFile}", messageId, targetFile);

        await using var fs = File.OpenWrite(targetFile);
        await content.CopyToAsync(fs, ct);
    }
}
