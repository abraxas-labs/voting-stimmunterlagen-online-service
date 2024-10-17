// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmunterlagen.Core.Models;

public class FileModel
{
    private readonly byte[] _data;

    public FileModel(byte[] data, string filename, string contentType)
    {
        _data = data;
        Filename = filename;
        ContentType = contentType;
    }

    public string Filename { get; }

    public string ContentType { get; }

    public async Task Write(PipeWriter writer, CancellationToken ct = default) => await writer.WriteAsync(_data, ct);
}
