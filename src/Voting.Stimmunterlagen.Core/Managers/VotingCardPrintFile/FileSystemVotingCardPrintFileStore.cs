// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.VotingCardPrintFile;

public class FileSystemVotingCardPrintFileStore : IVotingCardPrintFileStore
{
    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemVotingCardPrintFileStore> _logger;

    public FileSystemVotingCardPrintFileStore(ApiConfig config, ILogger<FileSystemVotingCardPrintFileStore> logger)
    {
        _logger = logger;
        _baseDirectory = Path.GetFullPath(config.VotingCardGenerator.OutDirectoryPath);
        Directory.CreateDirectory(_baseDirectory);
    }

    public Task SavePrintFile(string fileName, byte[] content, string messageId, CancellationToken ct)
    {
        var targetDirectory = Path.Combine(_baseDirectory, messageId);
        Directory.CreateDirectory(targetDirectory);
        var targetFile = Path.Combine(targetDirectory, fileName);
        _logger.LogDebug("writing print file to {MessageId}/{TargetFile}", messageId, targetFile);
        return File.WriteAllBytesAsync(targetFile, content, ct);
    }
}
