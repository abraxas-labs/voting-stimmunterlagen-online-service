// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmunterlagen.Core.Utils;

namespace Voting.Stimmunterlagen.Core.Managers.Stistat;

public class FileSystemStistatFileStore : IStistatFileStore
{
    private const string BaseDirectoryName = "stistat";

    private readonly string _baseDirectory;
    private readonly ILogger<FileSystemStistatFileStore> _logger;

    public FileSystemStistatFileStore(ILogger<FileSystemStistatFileStore> logger)
    {
        _logger = logger;
        _baseDirectory = Path.GetFullPath(Path.Join(StimmunterlagenOutDirectoryUtils.OutDirectoryBasePath, BaseDirectoryName));
    }

    public async Task Save(string fileName, Stream content, string messageType, CancellationToken ct)
    {
        var targetDirectory = Path.Combine(_baseDirectory, messageType);
        Directory.CreateDirectory(targetDirectory);
        var targetFile = Path.Combine(targetDirectory, fileName);
        _logger.LogInformation("Writing stistat export to {MessageType}:{TargetFile}", messageType, targetFile);
        await using var fileStream = File.Create(targetFile);
        await content.CopyToAsync(fileStream, ct);
    }
}
