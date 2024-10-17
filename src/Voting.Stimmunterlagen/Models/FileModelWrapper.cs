// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.Rest.Files;
using Voting.Stimmunterlagen.Core.Models;

namespace Voting.Stimmunterlagen.Models;

public class FileModelWrapper : IFile
{
    private readonly FileModel _fileModel;

    public FileModelWrapper(FileModel fileModel)
    {
        _fileModel = fileModel;
    }

    public string Filename => _fileModel.Filename;

    public string MimeType => _fileModel.ContentType;

    public Task Write(PipeWriter writer, CancellationToken ct = default) => _fileModel.Write(writer, ct);
}
