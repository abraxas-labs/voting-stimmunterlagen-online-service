// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Configuration;

namespace Voting.Stimmunterlagen.Core.Managers.EVoting;

public class FileSystemContestEVotingStore : IContestEVotingStore
{
    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemContestEVotingStore> _logger;

    public FileSystemContestEVotingStore(ApiConfig config, ILogger<FileSystemContestEVotingStore> logger)
    {
        _logger = logger;
        _baseDirectory = Path.GetFullPath(config.ContestEVotingExport.OutDirectoryPath);
        Directory.CreateDirectory(_baseDirectory);
    }

    public Task SaveContestEVotingExport(string fileName, byte[] content, string messageId, CancellationToken ct)
    {
        var targetDirectory = Path.Combine(_baseDirectory, messageId);
        Directory.CreateDirectory(targetDirectory);
        var targetFile = Path.Combine(targetDirectory, fileName);
        _logger.LogDebug("writing contest e-voting archive to {MessageId}/{TargetFile}", messageId, targetFile);
        return File.WriteAllBytesAsync(targetFile, content, ct);
    }
}
